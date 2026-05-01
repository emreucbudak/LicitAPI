using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Licit.AuthService.Application.Interfaces;

namespace Licit.AuthService.API.Services;

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

    public string? Email => FindClaim(ClaimTypes.Email, JwtRegisteredClaimNames.Email, "email");

    public string? TokenId => FindClaim(JwtRegisteredClaimNames.Jti, "jti");

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
