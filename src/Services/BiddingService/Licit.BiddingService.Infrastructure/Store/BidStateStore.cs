using Licit.BiddingService.Application.DTOs;
using Licit.BiddingService.Application.Interfaces;
using StackExchange.Redis;

namespace Licit.BiddingService.Infrastructure.Store
{
    public class BidStateStore(IConnectionMultiplexer connectionMultiplexer) : IBidStateStore
    {
        private const string ActiveStatus = "Active";
        private const string ActiveStatusTr = "Aktif";
        private const int RollbackTtlSeconds = 600;

        private const string CheckBidCanEnterScript = """
local stateKey = KEYS[1]
local amount = tonumber(ARGV[1])
local activeStatus = ARGV[2]
local activeStatusTr = ARGV[3]

local status = redis.call('HGET', stateKey, 'status')
local currentHighest = tonumber(redis.call('HGET', stateKey, 'highestBid') or '0')
local minimumIncrease = tonumber(redis.call('HGET', stateKey, 'minimumIncrease') or '0')
local version = tonumber(redis.call('HGET', stateKey, 'version') or '0')
local minimumRequired = currentHighest + minimumIncrease

if status == false then
  return {0, 'BID_STATE_NOT_FOUND', currentHighest, minimumRequired, version}
end

if status ~= activeStatus and status ~= activeStatusTr then
  return {0, 'AUCTION_NOT_ACTIVE', currentHighest, minimumRequired, version}
end

if amount < minimumRequired then
  return {0, 'BID_TOO_LOW', currentHighest, minimumRequired, version}
end

return {1, 'OK', currentHighest, minimumRequired, version}
""";

        private const string TrySetHighestBidScript = """
local stateKey = KEYS[1]
local rollbackKey = KEYS[2]
local amount = tonumber(ARGV[1])
local bidId = ARGV[2]
local bidderUserId = ARGV[3]
local placedAt = ARGV[4]
local activeStatus = ARGV[5]
local activeStatusTr = ARGV[6]
local rollbackTtlSeconds = tonumber(ARGV[7])

local status = redis.call('HGET', stateKey, 'status')
local currentHighest = tonumber(redis.call('HGET', stateKey, 'highestBid') or '0')
local minimumIncrease = tonumber(redis.call('HGET', stateKey, 'minimumIncrease') or '0')
local version = tonumber(redis.call('HGET', stateKey, 'version') or '0')
local minimumRequired = currentHighest + minimumIncrease

if status == false then
  return {0, 'BID_STATE_NOT_FOUND', currentHighest, version}
end

if status ~= activeStatus and status ~= activeStatusTr then
  return {0, 'AUCTION_NOT_ACTIVE', currentHighest, version}
end

if amount < minimumRequired then
  return {0, 'BID_TOO_LOW', currentHighest, version}
end

redis.call(
  'HSET',
  rollbackKey,
  'highestBid', currentHighest,
  'highestBidId', redis.call('HGET', stateKey, 'highestBidId') or '',
  'highestBidderUserId', redis.call('HGET', stateKey, 'highestBidderUserId') or '',
  'version', version)
redis.call('EXPIRE', rollbackKey, rollbackTtlSeconds)

local newVersion = redis.call('HINCRBY', stateKey, 'version', 1)
redis.call(
  'HSET',
  stateKey,
  'highestBid', amount,
  'highestBidId', bidId,
  'highestBidderUserId', bidderUserId,
  'updatedAt', placedAt)

return {1, 'OK', amount, newVersion}
""";

        private const string RollbackHighestBidScript = """
local stateKey = KEYS[1]
local rollbackKey = KEYS[2]
local bidId = ARGV[1]
local rolledBackAt = ARGV[2]

if redis.call('HGET', stateKey, 'highestBidId') ~= bidId then
  return {0, 'BID_STATE_CHANGED'}
end

if redis.call('EXISTS', rollbackKey) == 0 then
  return {0, 'ROLLBACK_STATE_NOT_FOUND'}
end

local previousHighest = redis.call('HGET', rollbackKey, 'highestBid') or '0'
local previousHighestBidId = redis.call('HGET', rollbackKey, 'highestBidId')
local previousHighestBidderUserId = redis.call('HGET', rollbackKey, 'highestBidderUserId')
local previousVersion = redis.call('HGET', rollbackKey, 'version') or '0'

redis.call(
  'HSET',
  stateKey,
  'highestBid', previousHighest,
  'version', previousVersion,
  'updatedAt', rolledBackAt)

if previousHighestBidId == false or previousHighestBidId == '' then
  redis.call('HDEL', stateKey, 'highestBidId')
else
  redis.call('HSET', stateKey, 'highestBidId', previousHighestBidId)
end

if previousHighestBidderUserId == false or previousHighestBidderUserId == '' then
  redis.call('HDEL', stateKey, 'highestBidderUserId')
else
  redis.call('HSET', stateKey, 'highestBidderUserId', previousHighestBidderUserId)
end

redis.call('DEL', rollbackKey)

return {1, 'OK'}
""";

        private readonly IDatabase _database = connectionMultiplexer.GetDatabase();

        public async Task<BidStateCheckResult> CheckBidCanEnterAsync(
            Guid auctionId,
            int amount,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = ToArray(await _database.ScriptEvaluateAsync(
                CheckBidCanEnterScript,
                [GetStateKey(auctionId)],
                [amount, ActiveStatus, ActiveStatusTr]));

            return IsSuccess(result[0])
                ? BidStateCheckResult.Accepted(ToInt32(result[2]), ToInt32(result[3]), ToInt32(result[4]))
                : BidStateCheckResult.Rejected(ToString(result[1]), ToInt32(result[2]), ToInt32(result[3]), ToInt32(result[4]));
        }

        public async Task<BidStateUpdateResult> TrySetHighestBidAsync(
            Guid auctionId,
            Guid bidId,
            Guid bidderUserId,
            int amount,
            DateTime placedAt,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = ToArray(await _database.ScriptEvaluateAsync(
                TrySetHighestBidScript,
                [GetStateKey(auctionId), GetRollbackKey(auctionId, bidId)],
                [
                    amount,
                    bidId.ToString("D"),
                    bidderUserId.ToString("D"),
                    placedAt.ToUniversalTime().ToString("O"),
                    ActiveStatus,
                    ActiveStatusTr,
                    RollbackTtlSeconds
                ]));

            return IsSuccess(result[0])
                ? BidStateUpdateResult.Updated(ToInt32(result[2]), ToInt32(result[3]))
                : BidStateUpdateResult.Rejected(ToString(result[1]), ToInt32(result[2]), ToInt32(result[3]));
        }

        public async Task<BidStateRollbackResult> TryRollbackHighestBidAsync(
            Guid auctionId,
            Guid bidId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = ToArray(await _database.ScriptEvaluateAsync(
                RollbackHighestBidScript,
                [GetStateKey(auctionId), GetRollbackKey(auctionId, bidId)],
                [bidId.ToString("D"), DateTime.UtcNow.ToString("O")]));

            return IsSuccess(result[0])
                ? BidStateRollbackResult.RolledBack()
                : BidStateRollbackResult.Failed(ToString(result[1]));
        }

        private static RedisKey GetStateKey(Guid auctionId)
            => $"auction:bidstate:{auctionId:N}";

        private static RedisKey GetRollbackKey(Guid auctionId, Guid bidId)
            => $"auction:bidstate:{auctionId:N}:rollback:{bidId:N}";

        private static bool IsSuccess(RedisResult result)
            => (long)result == 1;

        private static RedisResult[] ToArray(RedisResult result)
            => (RedisResult[]?)result
                ?? throw new InvalidOperationException("Redis script did not return an array result.");

        private static int ToInt32(RedisResult result)
            => checked((int)(long)result);

        private static string ToString(RedisResult result)
            => (string?)result ?? "UNKNOWN";
    }
}
