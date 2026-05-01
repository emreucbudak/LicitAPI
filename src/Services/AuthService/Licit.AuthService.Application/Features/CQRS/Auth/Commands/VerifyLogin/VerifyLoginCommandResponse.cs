namespace Licit.AuthService.Application.Features.CQRS.Auth.Commands.VerifyLogin;

public record VerifyLoginCommandResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);
