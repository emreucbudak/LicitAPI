namespace Licit.AuthService.Application.Features.CQRS.Auth.Commands.Login;

public record LoginCommandResponse(
    string TemporaryToken,
    DateTime ExpiresAt
);
