using System.Globalization;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Licit.Gateway.API.RateLimiting;

public sealed class RedisRateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRedisRateLimiter _rateLimiter;
    private readonly ILogger<RedisRateLimitingMiddleware> _logger;
    private readonly IReadOnlyList<RedisRateLimitPolicyOptions> _policies;
    private readonly IReadOnlyList<string> _bypassPaths;

    public RedisRateLimitingMiddleware(
        RequestDelegate next,
        IRedisRateLimiter rateLimiter,
        IOptions<RedisRateLimitingOptions> options,
        ILogger<RedisRateLimitingMiddleware> logger)
    {
        _next = next;
        _rateLimiter = rateLimiter;
        _logger = logger;
        _policies = options.Value.Policies
            .Where(policy => !string.IsNullOrWhiteSpace(policy.PathPrefix))
            .OrderByDescending(policy => policy.PathPrefix.Length)
            .ToArray();
        _bypassPaths = options.Value.BypassPaths
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(NormalizePath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestPath = NormalizePath(context.Request.Path.Value);

        if (ShouldBypass(requestPath))
        {
            await _next(context);
            return;
        }

        var matchedPolicy = ResolvePolicy(requestPath);
        if (matchedPolicy is null)
        {
            await _next(context);
            return;
        }

        var clientId = GetClientId(context);
        RedisRateLimitDecision decision;

        try
        {
            decision = await _rateLimiter.EvaluateAsync(matchedPolicy, clientId, context.RequestAborted);
        }
        catch (RedisException exception)
        {
            _logger.LogError(
                exception,
                "Redis rate limiting check failed for path {Path}. Request will continue without throttling.",
                requestPath);
            await _next(context);
            return;
        }

        WriteHeaders(context.Response, matchedPolicy, decision);

        if (decision.IsAllowed)
        {
            await _next(context);
            return;
        }

        var retryAfterSeconds = Math.Max(1, (int)Math.Ceiling(decision.RetryAfter.TotalSeconds));

        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.Response.ContentType = "application/json";
        context.Response.Headers["Retry-After"] = retryAfterSeconds.ToString(CultureInfo.InvariantCulture);

        _logger.LogWarning(
            "Rate limit exceeded for policy {Policy} and client {ClientId} on path {Path}.",
            matchedPolicy.Name,
            clientId,
            requestPath);

        await context.Response.WriteAsJsonAsync(
            new RateLimitExceededResponse(
                "rate_limit_exceeded",
                $"Too many requests for '{matchedPolicy.Name}'.",
                retryAfterSeconds),
            cancellationToken: context.RequestAborted);
    }

    private bool ShouldBypass(string requestPath) =>
        _bypassPaths.Any(path => MatchesPathPrefix(requestPath, path));

    private RedisRateLimitPolicyOptions? ResolvePolicy(string requestPath) =>
        _policies.FirstOrDefault(policy => MatchesPathPrefix(requestPath, NormalizePath(policy.PathPrefix)));

    private static string GetClientId(HttpContext context)
    {
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].ToString();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            var firstAddress = forwardedFor.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(firstAddress))
                return firstAddress;
        }

        var realIp = context.Request.Headers["X-Real-IP"].ToString();
        if (!string.IsNullOrWhiteSpace(realIp))
            return realIp.Trim();

        return context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
    }

    private static void WriteHeaders(
        HttpResponse response,
        RedisRateLimitPolicyOptions policy,
        RedisRateLimitDecision decision)
    {
        response.Headers["X-RateLimit-Limit"] = policy.PermitLimit.ToString(CultureInfo.InvariantCulture);
        response.Headers["X-RateLimit-Remaining"] = decision.Remaining.ToString(CultureInfo.InvariantCulture);
        response.Headers["X-RateLimit-Reset"] = Math.Max(0, (int)Math.Ceiling(decision.ResetAfter.TotalSeconds))
            .ToString(CultureInfo.InvariantCulture);
        response.Headers["X-RateLimit-Policy"] =
            $"{policy.Name};burst={policy.PermitLimit};refill={policy.PermitLimit}/{policy.WindowSeconds}s";
    }

    private static bool MatchesPathPrefix(string requestPath, string configuredPrefix)
    {
        if (configuredPrefix == "/")
            return true;

        if (string.Equals(requestPath, configuredPrefix, StringComparison.OrdinalIgnoreCase))
            return true;

        if (!requestPath.StartsWith(configuredPrefix, StringComparison.OrdinalIgnoreCase))
            return false;

        return requestPath.Length > configuredPrefix.Length &&
               requestPath[configuredPrefix.Length] == '/';
    }

    private static string NormalizePath(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "/";

        var normalized = value.Trim();

        if (!normalized.StartsWith('/'))
            normalized = "/" + normalized;

        if (normalized.Length > 1)
            normalized = normalized.TrimEnd('/');

        return normalized;
    }

    private sealed record RateLimitExceededResponse(
        string Error,
        string Message,
        int RetryAfterSeconds);
}
