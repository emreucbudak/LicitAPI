using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Licit.Gateway.API.Notifications;

public static class NotificationUser
{
    public static string? ResolveUserId(ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        foreach (var claimType in new[]
                 {
                     ClaimTypes.NameIdentifier,
                     JwtRegisteredClaimNames.Sub,
                     "sub",
                     "nameid",
                     ClaimTypes.Name,
                     JwtRegisteredClaimNames.Email,
                     ClaimTypes.Email
                 })
        {
            var value = user.FindFirst(claimType)?.Value;
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }
}
