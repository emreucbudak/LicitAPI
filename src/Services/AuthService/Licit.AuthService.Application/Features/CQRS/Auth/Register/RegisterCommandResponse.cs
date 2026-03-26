namespace Licit.AuthService.Application.Features.CQRS.Auth.Register;

public record RegisterCommandResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);
