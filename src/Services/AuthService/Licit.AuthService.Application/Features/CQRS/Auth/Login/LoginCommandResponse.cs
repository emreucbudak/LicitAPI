namespace Licit.AuthService.Application.Features.CQRS.Auth.Login;

public record LoginCommandResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);
