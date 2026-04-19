using System.Text.Json;
using Licit.AuthService.Application.DTOs;
using Licit.AuthService.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace Licit.AuthService.Infrastructure.Services;

public class RedisRegisterVerificationStore(IDistributedCache cache) : IRegisterVerificationStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task StoreAsync(
        string temporaryToken,
        PendingRegistrationVerification verification,
        TimeSpan lifetime,
        CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(verification, JsonOptions);

        await cache.SetStringAsync(
            BuildCacheKey(temporaryToken),
            payload,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = lifetime
            },
            cancellationToken);
    }

    public async Task<PendingRegistrationVerification?> GetAsync(
        string temporaryToken,
        CancellationToken cancellationToken = default)
    {
        var payload = await cache.GetStringAsync(BuildCacheKey(temporaryToken), cancellationToken);
        if (string.IsNullOrWhiteSpace(payload))
            return null;

        return JsonSerializer.Deserialize<PendingRegistrationVerification>(payload, JsonOptions);
    }

    public Task RemoveAsync(string temporaryToken, CancellationToken cancellationToken = default) =>
        cache.RemoveAsync(BuildCacheKey(temporaryToken), cancellationToken);

    private static string BuildCacheKey(string temporaryToken) =>
        $"auth:register-verification:{temporaryToken}";
}
