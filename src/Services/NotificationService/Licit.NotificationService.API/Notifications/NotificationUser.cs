using System.Security.Claims;

namespace Licit.NotificationService.API.Notifications;

public static class NotificationUser
{
    public static string? ResolveUserId(ClaimsPrincipal principal)
    {
        foreach (var claimType in new[]
                 {
                     ClaimTypes.NameIdentifier,
                     "sub",
                     "userId",
                     "user_id",
                     "nameid",
                     "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
                 })
        {
            var value = principal.FindFirst(claimType)?.Value;
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }
}
