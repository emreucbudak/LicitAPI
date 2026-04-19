namespace Licit.Gateway.API.RateLimiting;

public sealed class RedisRateLimitingOptions
{
    public RedisConnectionOptions Redis { get; set; } = new();
    public List<string> BypassPaths { get; set; } = [];
    public List<RedisRateLimitPolicyOptions> Policies { get; set; } = [];
}

public sealed class RedisConnectionOptions
{
    public string? ConnectionString { get; set; }
    public string KeyPrefix { get; set; } = "gateway:ratelimit";
}

public sealed class RedisRateLimitPolicyOptions
{
    public string Name { get; set; } = string.Empty;
    public string PathPrefix { get; set; } = string.Empty;
    public int PermitLimit { get; set; }
    public int WindowSeconds { get; set; }
}
