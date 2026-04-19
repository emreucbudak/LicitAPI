namespace Licit.Gateway.API.RateLimiting;

public interface IRedisRateLimiter
{
    Task<RedisRateLimitDecision> EvaluateAsync(
        RedisRateLimitPolicyOptions policy,
        string clientId,
        CancellationToken cancellationToken = default);
}
