using System.Text.Json;
using Licit.AuthService.Application.DTOs;
using Licit.AuthService.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace Licit.AuthService.Infrastructure.Services;

public class RedisRegisterVerificationStore(IDistributedCache cache) : IRegisterVerificationStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task StoreAsync(
        string email,
        PendingRegistrationVerification verification,
        TimeSpan lifetime,
        CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(verification, JsonOptions);

        await cache.SetStringAsync(
            BuildCacheKey(email),
            payload,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = lifetime
            },
            cancellationToken);
    }

    public async Task<PendingRegistrationVerification?> GetAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var payload = await cache.GetStringAsync(BuildCacheKey(email), cancellationToken);
        if (string.IsNullOrWhiteSpace(payload))
            return null;

        return JsonSerializer.Deserialize<PendingRegistrationVerification>(payload, JsonOptions);
    }

    public Task RemoveAsync(string email, CancellationToken cancellationToken = default) =>
        cache.RemoveAsync(BuildCacheKey(email), cancellationToken);

    private static string BuildCacheKey(string email) =>
        $"auth:register-verification:{email.Trim().ToUpperInvariant()}";
}
