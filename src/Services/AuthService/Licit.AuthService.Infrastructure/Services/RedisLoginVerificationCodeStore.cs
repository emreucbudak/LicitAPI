using System.Text.Json;
using Licit.AuthService.Application.DTOs;
using Licit.AuthService.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace Licit.AuthService.Infrastructure.Services;

public class RedisLoginVerificationCodeStore(IDistributedCache cache) : ILoginVerificationCodeStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task StoreAsync(
        string email,
        LoginVerificationChallenge challenge,
        TimeSpan lifetime,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildCacheKey(email);
        var payload = JsonSerializer.Serialize(challenge, JsonOptions);

        await cache.SetStringAsync(
            cacheKey,
            payload,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = lifetime
            },
            cancellationToken);
    }

    public async Task<LoginVerificationChallenge?> GetAsync(string email, CancellationToken cancellationToken = default)
    {
        var payload = await cache.GetStringAsync(BuildCacheKey(email), cancellationToken);
        if (string.IsNullOrWhiteSpace(payload))
            return null;

        return JsonSerializer.Deserialize<LoginVerificationChallenge>(payload, JsonOptions);
    }

    public Task RemoveAsync(string email, CancellationToken cancellationToken = default) =>
        cache.RemoveAsync(BuildCacheKey(email), cancellationToken);

    private static string BuildCacheKey(string email) =>
        $"auth:login-2fa:{email.Trim().ToUpperInvariant()}";
}
