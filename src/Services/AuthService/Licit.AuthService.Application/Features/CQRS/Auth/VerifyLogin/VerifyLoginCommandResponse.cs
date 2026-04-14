namespace Licit.AuthService.Application.Features.CQRS.Auth.VerifyLogin;

public record VerifyLoginCommandResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);
