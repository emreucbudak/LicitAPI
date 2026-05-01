using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Licit.TenderingService.Application.Interfaces;

namespace Licit.TenderingService.API.Services;

public class HttpContextCurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid? UserId
    {
        get
        {
            var value = FindClaim(ClaimTypes.NameIdentifier, JwtRegisteredClaimNames.Sub, "sub");
            return Guid.TryParse(value, out var userId) ? userId : null;
        }
    }

    private string? FindClaim(params string[] claimTypes)
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user is null)
            return null;

        return claimTypes
            .Select(claimType => user.FindFirst(claimType)?.Value)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }
}
