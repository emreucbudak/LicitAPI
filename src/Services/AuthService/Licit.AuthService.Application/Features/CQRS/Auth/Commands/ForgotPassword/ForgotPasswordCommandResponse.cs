namespace Licit.AuthService.Application.Features.CQRS.Auth.Commands.ForgotPassword;

public record ForgotPasswordCommandResponse(
    string TemporaryToken,
    DateTime ExpiresAt,
    string Email
);
