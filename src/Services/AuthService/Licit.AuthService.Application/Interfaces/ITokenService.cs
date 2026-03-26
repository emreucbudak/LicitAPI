using Licit.AuthService.Domain.Entities;

namespace Licit.AuthService.Application.Interfaces;

public interface ITokenService
{
    Task<string> GenerateAccessTokenAsync(ApplicationUser user);
    string GenerateRefreshToken(Guid userId);
    Guid? ValidateRefreshToken(string refreshToken);
}
