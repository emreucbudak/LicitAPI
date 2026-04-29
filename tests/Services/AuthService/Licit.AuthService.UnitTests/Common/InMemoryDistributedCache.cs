using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Distributed;

namespace Licit.AuthService.UnitTests.Common;

public class InMemoryDistributedCache : IDistributedCache
{
    private readonly ConcurrentDictionary<string, CacheEntry> _entries = new();

    public byte[]? Get(string key)
    {
        if (!_entries.TryGetValue(key, out var entry))
            return null;

        if (entry.ExpiresAt is not null && entry.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            _entries.TryRemove(key, out _);
            return null;
        }

        return entry.Value;
    }

    public Task<byte[]?> GetAsync(string key, CancellationToken token = default)
        => Task.FromResult(Get(key));

    public void Refresh(string key)
    {
    }

    public Task RefreshAsync(string key, CancellationToken token = default)
        => Task.CompletedTask;

    public void Remove(string key)
        => _entries.TryRemove(key, out _);

    public Task RemoveAsync(string key, CancellationToken token = default)
    {
        Remove(key);
        return Task.CompletedTask;
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        var expiresAt = options.AbsoluteExpirationRelativeToNow is { } relativeExpiration
            ? DateTimeOffset.UtcNow.Add(relativeExpiration)
            : options.AbsoluteExpiration;

        _entries[key] = new CacheEntry(value, expiresAt);
    }

    public Task SetAsync(
        string key,
        byte[] value,
        DistributedCacheEntryOptions options,
        CancellationToken token = default)
    {
        Set(key, value, options);
        return Task.CompletedTask;
    }

    private sealed record CacheEntry(byte[] Value, DateTimeOffset? ExpiresAt);
}
