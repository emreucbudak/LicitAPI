using Licit.AuthService.Domain.Entities;

namespace Licit.AuthService.Application.Interfaces;

public interface ITokenService
{
    Task<string> GenerateAccessTokenAsync(ApplicationUser user);
    string GenerateTemporaryLoginToken(ApplicationUser user, DateTime expiresAt, string challengeId);
    string GenerateRefreshToken(Guid userId);
    Guid? ValidateRefreshToken(string refreshToken);
}
