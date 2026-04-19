namespace Licit.Gateway.API.RateLimiting;

public sealed record RedisRateLimitDecision(
    bool IsAllowed,
    int Remaining,
    TimeSpan RetryAfter,
    TimeSpan ResetAfter);
