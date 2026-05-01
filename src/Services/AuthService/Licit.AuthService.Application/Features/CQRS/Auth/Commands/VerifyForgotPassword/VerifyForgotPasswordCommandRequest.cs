using FlashMediator;

namespace Licit.AuthService.Application.Features.CQRS.Auth.Commands.VerifyForgotPassword;

public record VerifyForgotPasswordCommandRequest(
    string TemporaryToken,
    string Code
) : IRequest<VerifyForgotPasswordCommandResponse>;
