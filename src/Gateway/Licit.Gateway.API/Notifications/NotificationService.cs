using Microsoft.AspNetCore.SignalR;

namespace Licit.Gateway.API.Notifications;

public sealed class NotificationService(
    INotificationStore store,
    IHubContext<NotificationHub> hubContext) : INotificationService
{
    public async Task<IReadOnlyList<NotificationDto>> GetRecentAsync(
        string userId,
        int take,
        CancellationToken cancellationToken)
    {
        var items = await store.GetRecentAsync(userId, take, cancellationToken);

        return items.Select(ToDto).ToArray();
    }

    public Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken) =>
        store.GetUnreadCountAsync(userId, cancellationToken);

    public async Task<NotificationDto?> PublishAsync(CreateNotificationRequest request, CancellationToken cancellationToken)
    {
        var item = await store.AddAsync(request, cancellationToken);
        if (item is null)
        {
            return null;
        }

        var dto = ToDto(item);
        await hubContext.Clients.User(item.UserId).SendAsync("NotificationReceived", dto, cancellationToken);
        await SendUnreadCountAsync(item.UserId, cancellationToken);

        return dto;
    }

    public async Task<NotificationDto?> MarkReadAsync(
        string userId,
        string notificationId,
        CancellationToken cancellationToken)
    {
        var item = await store.MarkReadAsync(userId, notificationId, cancellationToken);
        if (item is null)
        {
            return null;
        }

        await SendUnreadCountAsync(userId, cancellationToken);

        return ToDto(item);
    }

    public async Task<int> MarkAllReadAsync(string userId, CancellationToken cancellationToken)
    {
        var updatedCount = await store.MarkAllReadAsync(userId, cancellationToken);
        await SendUnreadCountAsync(userId, cancellationToken);

        return updatedCount;
    }

    private async Task SendUnreadCountAsync(string userId, CancellationToken cancellationToken)
    {
        var count = await store.GetUnreadCountAsync(userId, cancellationToken);
        await hubContext.Clients.User(userId).SendAsync("UnreadCountChanged", new UnreadCountResponse(count), cancellationToken);
    }

    private static NotificationDto ToDto(NotificationItem item) =>
        new(
            item.Id,
            item.Type,
            item.Title,
            item.Body,
            item.LinkUrl,
            item.Data,
            item.CreatedAt,
            item.ReadAt,
            item.IsRead);
}
