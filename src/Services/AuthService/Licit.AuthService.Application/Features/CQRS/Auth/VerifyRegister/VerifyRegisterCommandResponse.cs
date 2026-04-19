namespace Licit.AuthService.Application.Features.CQRS.Auth.VerifyRegister;

public record VerifyRegisterCommandResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);
