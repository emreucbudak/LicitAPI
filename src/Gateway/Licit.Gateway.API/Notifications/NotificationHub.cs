using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Licit.Gateway.API.Notifications;

[Authorize(Policy = NotificationAuth.AccessTokenPolicy)]
public sealed class NotificationHub : Hub
{
}
