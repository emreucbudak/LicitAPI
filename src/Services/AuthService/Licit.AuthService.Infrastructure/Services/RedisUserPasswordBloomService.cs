using Licit.AuthService.Application.DTOs;
using Licit.AuthService.Application.Interfaces;
using StackExchange.Redis;

namespace Licit.AuthService.Infrastructure.Services;

public class RedisUserPasswordBloomService(
    IConnectionMultiplexer connectionMultiplexer,
    AuthBloomFilterSettings settings) : IUserPasswordBloomService
{
    private readonly IDatabase _database = connectionMultiplexer.GetDatabase();

    public async Task<bool> MayContainAsync(Guid userId, string fingerprint, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await _database.ExecuteAsync("BF.EXISTS", GetBloomKey(userId), fingerprint);
        return (int)result == 1;
    }

    public async Task<IReadOnlyList<string>> GetFingerprintsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var values = await _database.ListRangeAsync(GetExactFingerprintsKey(userId));
        return values
            .Where(x => x.HasValue)
            .Select(x => x.ToString())
            .ToArray();
    }

    public async Task SetFingerprintsAsync(
        Guid userId,
        IReadOnlyCollection<string> fingerprints,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var distinctFingerprints = fingerprints
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .Take(4)
            .ToArray();

        var bloomKey = GetBloomKey(userId);
        var exactKey = GetExactFingerprintsKey(userId);

        await _database.KeyDeleteAsync(new RedisKey[] { bloomKey, exactKey });

        if (distinctFingerprints.Length == 0)
            return;

        await EnsureFilterAsync(bloomKey);
        foreach (var fingerprint in distinctFingerprints)
            await _database.ExecuteAsync("BF.ADD", bloomKey, fingerprint);

        await _database.ListRightPushAsync(exactKey, distinctFingerprints.Select(x => (RedisValue)x).ToArray());
    }

    private async Task EnsureFilterAsync(string key)
    {
        try
        {
            await _database.ExecuteAsync("BF.RESERVE", key, settings.ErrorRate, settings.PasswordCapacity);
        }
        catch (RedisServerException ex) when (ex.Message.Contains("exists", StringComparison.OrdinalIgnoreCase))
        {
        }
    }

    private string GetBloomKey(Guid userId) => $"{settings.PasswordsKeyPrefix}:{userId:N}";

    private string GetExactFingerprintsKey(Guid userId) => $"{settings.PasswordFingerprintsKeyPrefix}:{userId:N}";
}
