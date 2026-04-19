using FlashMediator;

namespace Licit.AuthService.Application.Features.CQRS.Auth.ResetForgotPassword;

public record ResetForgotPasswordCommandRequest(
    string TemporaryToken,
    string NewPassword
) : IRequest<ResetForgotPasswordCommandResponse>;
