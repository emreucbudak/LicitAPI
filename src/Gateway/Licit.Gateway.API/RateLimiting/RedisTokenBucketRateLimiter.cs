using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Licit.Gateway.API.RateLimiting;

public sealed class RedisTokenBucketRateLimiter : IRedisRateLimiter
{
    private const string TokenBucketScript = """
        local capacity = tonumber(ARGV[1])
        local refill_tokens = tonumber(ARGV[2])
        local refill_period_ms = tonumber(ARGV[3])
        local requested_tokens = tonumber(ARGV[4])

        local now_parts = redis.call('TIME')
        local now = (tonumber(now_parts[1]) * 1000) + math.floor(tonumber(now_parts[2]) / 1000)

        local bucket = redis.call('HMGET', KEYS[1], 'tokens', 'ts')
        local tokens = tonumber(bucket[1])
        local ts = tonumber(bucket[2])

        if tokens == nil then
            tokens = capacity
            ts = now
        end

        if ts == nil or ts > now then
            ts = now
        end

        local elapsed = math.max(0, now - ts)
        if elapsed > 0 then
            local replenished = (elapsed * refill_tokens) / refill_period_ms
            tokens = math.min(capacity, tokens + replenished)
        end

        local allowed = 0
        if tokens >= requested_tokens then
            tokens = tokens - requested_tokens
            allowed = 1
        end

        local retry_after_ms = 0
        if tokens < requested_tokens then
            retry_after_ms = math.ceil(((requested_tokens - tokens) * refill_period_ms) / refill_tokens)
        end

        local reset_after_ms = 0
        if tokens < capacity then
            reset_after_ms = math.ceil(((capacity - tokens) * refill_period_ms) / refill_tokens)
        end

        redis.call('HSET', KEYS[1], 'tokens', string.format('%.17g', tokens), 'ts', tostring(now))

        local ttl_ms = math.max(1, reset_after_ms)
        if ttl_ms == 1 and tokens >= capacity then
            ttl_ms = refill_period_ms
        end

        redis.call('PEXPIRE', KEYS[1], ttl_ms)

        return {
            allowed,
            string.format('%.17g', tokens),
            retry_after_ms,
            reset_after_ms
        }
        """;

    private readonly IDatabase _database;
    private readonly string _keyPrefix;

    public RedisTokenBucketRateLimiter(
        IConnectionMultiplexer connectionMultiplexer,
        IOptions<RedisRateLimitingOptions> options)
    {
        _database = connectionMultiplexer.GetDatabase();
        _keyPrefix = options.Value.Redis.KeyPrefix;
    }

    public async Task<RedisRateLimitDecision> EvaluateAsync(
        RedisRateLimitPolicyOptions policy,
        string clientId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var key = BuildRedisKey(policy.Name, clientId);
        var windowMilliseconds = Math.Max(1, policy.WindowSeconds * 1000);
        var scriptResult = await _database.ScriptEvaluateAsync(
            TokenBucketScript,
            [key],
            [policy.PermitLimit, policy.PermitLimit, windowMilliseconds, 1]);
        var redisResult = (RedisResult[]?)scriptResult
            ?? throw new RedisException("Redis rate limiting script returned no result.");

        if (redisResult.Length < 4 || redisResult[0].IsNull || redisResult[1].IsNull ||
            redisResult[2].IsNull || redisResult[3].IsNull)
            throw new RedisException("Redis rate limiting script returned an invalid response.");

        var isAllowed = ReadInt(redisResult[0]) == 1;
        var remainingTokens = Math.Max(0d, ReadDouble(redisResult[1]));
        var retryAfterMilliseconds = Math.Max(0, ReadInt(redisResult[2]));
        var resetAfterMilliseconds = Math.Max(0, ReadInt(redisResult[3]));

        return new RedisRateLimitDecision(
            IsAllowed: isAllowed,
            Remaining: Math.Max(0, (int)Math.Floor(remainingTokens)),
            RetryAfter: TimeSpan.FromMilliseconds(retryAfterMilliseconds),
            ResetAfter: TimeSpan.FromMilliseconds(resetAfterMilliseconds));
    }

    private string BuildRedisKey(string policyName, string clientId) =>
        $"{_keyPrefix}:{policyName}:{HashClientId(clientId)}";

    private static string HashClientId(string clientId)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(clientId));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static int ReadInt(RedisResult result)
    {
        var value = result.ToString();
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
            throw new RedisException($"Redis rate limiting script returned a non-integer value: {value}");

        return parsed;
    }

    private static double ReadDouble(RedisResult result)
    {
        var value = result.ToString();
        if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
            throw new RedisException($"Redis rate limiting script returned a non-numeric value: {value}");

        return parsed;
    }
}
