using FlashMediator;

namespace Licit.AuthService.Application.Features.CQRS.Auth.Commands.VerifyRegister;

public record VerifyRegisterCommandRequest(
    string Email,
    string Code
) : IRequest<VerifyRegisterCommandResponse>;
