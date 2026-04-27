namespace Licit.AuthService.Application.Features.CQRS.Auth.Register;

public record RegisterCommandResponse(
    string Email,
    DateTime ExpiresAt
);
