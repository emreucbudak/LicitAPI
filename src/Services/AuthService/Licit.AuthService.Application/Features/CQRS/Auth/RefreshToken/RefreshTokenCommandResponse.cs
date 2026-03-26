namespace Licit.AuthService.Application.Features.CQRS.Auth.RefreshToken;

public record RefreshTokenCommandResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);
