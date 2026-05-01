namespace Licit.AuthService.Application.Features.CQRS.Auth.Commands.Register;

public record RegisterCommandResponse(
    string Email,
    DateTime ExpiresAt
);
