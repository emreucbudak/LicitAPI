using System.Security.Cryptography;
using System.Text;
using Licit.WalletService.Application.Interfaces;
using StackExchange.Redis;

namespace Licit.WalletService.Infrastructure.Services;

public class RedisDepositIdempotencyStore(IConnectionMultiplexer connectionMultiplexer) : IDepositIdempotencyStore
{
    private readonly IDatabase _database = connectionMultiplexer.GetDatabase();

    public async Task<bool> TryReserveAsync(
        Guid userId,
        string idempotencyKey,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await _database.StringSetAsync(
            GetRedisKey(userId, idempotencyKey),
            "reserved",
            ttl,
            When.NotExists);
    }

    public async Task ReleaseAsync(
        Guid userId,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _database.KeyDeleteAsync(GetRedisKey(userId, idempotencyKey));
    }

    private static string GetRedisKey(Guid userId, string idempotencyKey)
    {
        var keyHash = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(idempotencyKey.Trim())))
            .ToLowerInvariant();

        return $"wallet:deposit:idempotency:{userId:N}:{keyHash}";
    }
}
