namespace Licit.Gateway.API.Notifications;

public interface INotificationStore
{
    Task<IReadOnlyList<NotificationItem>> GetRecentAsync(string userId, int take, CancellationToken cancellationToken);

    Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken);

    Task<NotificationItem?> AddAsync(CreateNotificationRequest request, CancellationToken cancellationToken);

    Task<NotificationItem?> MarkReadAsync(string userId, string notificationId, CancellationToken cancellationToken);

    Task<int> MarkAllReadAsync(string userId, CancellationToken cancellationToken);
}
