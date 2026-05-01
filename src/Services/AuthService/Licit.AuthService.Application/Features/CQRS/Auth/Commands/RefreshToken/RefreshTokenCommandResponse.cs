namespace Licit.AuthService.Application.Features.CQRS.Auth.Commands.RefreshToken;

public record RefreshTokenCommandResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);
