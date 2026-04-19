namespace Licit.AuthService.Application.Features.CQRS.Auth.Register;

public record RegisterCommandResponse(
    string TemporaryToken,
    DateTime ExpiresAt,
    string Email
);
