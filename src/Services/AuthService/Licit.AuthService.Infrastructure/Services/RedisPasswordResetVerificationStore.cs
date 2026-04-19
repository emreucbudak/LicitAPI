using System.Text.Json;
using Licit.AuthService.Application.DTOs;
using Licit.AuthService.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace Licit.AuthService.Infrastructure.Services;

public class RedisPasswordResetVerificationStore(IDistributedCache cache) : IPasswordResetVerificationStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task StoreAsync(
        string temporaryToken,
        PasswordResetVerificationChallenge challenge,
        TimeSpan lifetime,
        CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(challenge, JsonOptions);

        await cache.SetStringAsync(
            BuildCacheKey(temporaryToken),
            payload,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = lifetime
            },
            cancellationToken);
    }

    public async Task<PasswordResetVerificationChallenge?> GetAsync(
        string temporaryToken,
        CancellationToken cancellationToken = default)
    {
        var payload = await cache.GetStringAsync(BuildCacheKey(temporaryToken), cancellationToken);
        if (string.IsNullOrWhiteSpace(payload))
            return null;

        return JsonSerializer.Deserialize<PasswordResetVerificationChallenge>(payload, JsonOptions);
    }

    public Task RemoveAsync(string temporaryToken, CancellationToken cancellationToken = default) =>
        cache.RemoveAsync(BuildCacheKey(temporaryToken), cancellationToken);

    private static string BuildCacheKey(string temporaryToken) =>
        $"auth:password-reset:{temporaryToken}";
}
