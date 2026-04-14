namespace Licit.AuthService.Application.Features.CQRS.Auth.Login;

public record LoginCommandResponse(
    string TemporaryToken,
    DateTime ExpiresAt
);
