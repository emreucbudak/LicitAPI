namespace Licit.AuthService.Application.Features.CQRS.Auth.ForgotPassword;

public record ForgotPasswordCommandResponse(
    string TemporaryToken,
    DateTime ExpiresAt,
    string Email
);
