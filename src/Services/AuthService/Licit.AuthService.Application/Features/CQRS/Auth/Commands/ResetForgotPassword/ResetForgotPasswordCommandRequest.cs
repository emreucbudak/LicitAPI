using FlashMediator;

namespace Licit.AuthService.Application.Features.CQRS.Auth.Commands.ResetForgotPassword;

public record ResetForgotPasswordCommandRequest(
    string TemporaryToken,
    string NewPassword
) : IRequest<ResetForgotPasswordCommandResponse>;
