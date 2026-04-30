using Microsoft.AspNetCore.SignalR;

namespace Licit.NotificationService.API.Notifications;

public sealed class NotificationUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection) =>
        NotificationUser.ResolveUserId(connection.User);
}
