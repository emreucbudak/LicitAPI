using System.Collections.Concurrent;

namespace Licit.Gateway.API.Notifications;

public sealed class InMemoryNotificationStore : INotificationStore
{
    private const int MaxItemsPerUser = 200;
    private readonly ConcurrentDictionary<string, List<NotificationItem>> _notifications = new(StringComparer.Ordinal);
    private readonly object _syncRoot = new();

    public Task<IReadOnlyList<NotificationItem>> GetRecentAsync(string userId, int take, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            if (!_notifications.TryGetValue(userId, out var items))
            {
                return Task.FromResult<IReadOnlyList<NotificationItem>>(Array.Empty<NotificationItem>());
            }

            return Task.FromResult<IReadOnlyList<NotificationItem>>(
                items
                    .OrderByDescending(item => item.CreatedAt)
                    .Take(take)
                    .ToArray());
        }
    }

    public Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            var count = _notifications.TryGetValue(userId, out var items)
                ? items.Count(item => item.ReadAt is null)
                : 0;

            return Task.FromResult(count);
        }
    }

    public Task<NotificationItem?> AddAsync(CreateNotificationRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(request.UserId) ||
            string.IsNullOrWhiteSpace(request.Type) ||
            string.IsNullOrWhiteSpace(request.Title))
        {
            return Task.FromResult<NotificationItem?>(null);
        }

        var item = new NotificationItem(
            Guid.NewGuid().ToString("N"),
            request.UserId,
            request.Type,
            request.Title,
            request.Body,
            request.LinkUrl,
            request.Data,
            DateTimeOffset.UtcNow,
            null);

        lock (_syncRoot)
        {
            var userItems = _notifications.GetOrAdd(request.UserId, _ => []);
            userItems.Insert(0, item);

            if (userItems.Count > MaxItemsPerUser)
            {
                userItems.RemoveRange(MaxItemsPerUser, userItems.Count - MaxItemsPerUser);
            }
        }

        return Task.FromResult<NotificationItem?>(item);
    }

    public Task<NotificationItem?> MarkReadAsync(string userId, string notificationId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            if (!_notifications.TryGetValue(userId, out var items))
            {
                return Task.FromResult<NotificationItem?>(null);
            }

            var index = items.FindIndex(item => string.Equals(item.Id, notificationId, StringComparison.Ordinal));
            if (index < 0)
            {
                return Task.FromResult<NotificationItem?>(null);
            }

            var existing = items[index];
            if (existing.ReadAt is not null)
            {
                return Task.FromResult<NotificationItem?>(existing);
            }

            var updated = existing with { ReadAt = DateTimeOffset.UtcNow };
            items[index] = updated;

            return Task.FromResult<NotificationItem?>(updated);
        }
    }

    public Task<int> MarkAllReadAsync(string userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            if (!_notifications.TryGetValue(userId, out var items))
            {
                return Task.FromResult(0);
            }

            var updatedCount = 0;
            for (var i = 0; i < items.Count; i++)
            {
                if (items[i].ReadAt is not null)
                {
                    continue;
                }

                items[i] = items[i] with { ReadAt = DateTimeOffset.UtcNow };
                updatedCount++;
            }

            return Task.FromResult(updatedCount);
        }
    }
}
