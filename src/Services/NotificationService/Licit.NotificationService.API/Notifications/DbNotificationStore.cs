using Licit.NotificationService.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Licit.NotificationService.API.Notifications;

public sealed class DbNotificationStore(NotificationDbContext dbContext) : INotificationStore
{
    private const int MaxItemsPerUser = 200;

    public async Task<IReadOnlyList<NotificationItem>> GetRecentAsync(
        string userId,
        int take,
        CancellationToken cancellationToken)
    {
        return await dbContext.Notifications
            .AsNoTracking()
            .Where(notification => notification.UserId == userId)
            .OrderByDescending(notification => notification.CreatedAt)
            .Take(take)
            .ToArrayAsync(cancellationToken);
    }

    public Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken) =>
        dbContext.Notifications
            .AsNoTracking()
            .CountAsync(notification => notification.UserId == userId && notification.ReadAt == null, cancellationToken);

    public async Task<NotificationItem?> AddAsync(CreateNotificationRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId) ||
            string.IsNullOrWhiteSpace(request.Type) ||
            string.IsNullOrWhiteSpace(request.Title))
        {
            return null;
        }

        var item = new NotificationItem
        {
            Id = Guid.NewGuid().ToString("N"),
            UserId = request.UserId,
            Type = request.Type.Trim(),
            Title = request.Title.Trim(),
            Body = string.IsNullOrWhiteSpace(request.Body) ? null : request.Body.Trim(),
            LinkUrl = string.IsNullOrWhiteSpace(request.LinkUrl) ? null : request.LinkUrl.Trim(),
            Data = request.Data?.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal),
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.Notifications.Add(item);
        await dbContext.SaveChangesAsync(cancellationToken);
        await TrimUserNotificationsAsync(item.UserId, cancellationToken);

        return item;
    }

    public async Task<NotificationItem?> MarkReadAsync(
        string userId,
        string notificationId,
        CancellationToken cancellationToken)
    {
        var item = await dbContext.Notifications
            .FirstOrDefaultAsync(
                notification => notification.UserId == userId && notification.Id == notificationId,
                cancellationToken);

        if (item is null)
        {
            return null;
        }

        if (item.ReadAt is null)
        {
            item.ReadAt = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return item;
    }

    public async Task<int> MarkAllReadAsync(string userId, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        return await dbContext.Notifications
            .Where(notification => notification.UserId == userId && notification.ReadAt == null)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(notification => notification.ReadAt, now),
                cancellationToken);
    }

    private async Task TrimUserNotificationsAsync(string userId, CancellationToken cancellationToken)
    {
        var staleIds = await dbContext.Notifications
            .Where(notification => notification.UserId == userId)
            .OrderByDescending(notification => notification.CreatedAt)
            .Skip(MaxItemsPerUser)
            .Select(notification => notification.Id)
            .ToArrayAsync(cancellationToken);

        if (staleIds.Length == 0)
        {
            return;
        }

        await dbContext.Notifications
            .Where(notification => staleIds.Contains(notification.Id))
            .ExecuteDeleteAsync(cancellationToken);
    }
}
