namespace Licit.NotificationService.API.Notifications;

public interface INotificationService
{
    Task<IReadOnlyList<NotificationDto>> GetRecentAsync(string userId, int take, CancellationToken cancellationToken);

    Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken);

    Task<NotificationDto?> PublishAsync(CreateNotificationRequest request, CancellationToken cancellationToken);

    Task<NotificationDto?> MarkReadAsync(string userId, string notificationId, CancellationToken cancellationToken);

    Task<int> MarkAllReadAsync(string userId, CancellationToken cancellationToken);
}
