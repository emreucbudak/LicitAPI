using Licit.AuthService.Domain.Entities;
using Licit.AuthService.Application.DTOs;

namespace Licit.AuthService.Application.Interfaces;

public interface ITokenService
{
    Task<string> GenerateAccessTokenAsync(ApplicationUser user);
    string GenerateTemporaryLoginToken(ApplicationUser user, DateTime expiresAt, string challengeId);
    string GenerateTemporaryRegisterToken(string email, DateTime expiresAt, string challengeId);
    string GenerateTemporaryPasswordResetToken(string email, DateTime expiresAt, string challengeId);
    string GenerateRefreshToken(Guid userId);
    Guid? ValidateRefreshToken(string refreshToken);
    TemporaryTokenPayload? ValidateTemporaryToken(string temporaryToken, string expectedTokenType);
}
