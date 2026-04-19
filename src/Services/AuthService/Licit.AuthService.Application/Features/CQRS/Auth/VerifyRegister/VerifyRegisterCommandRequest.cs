using FlashMediator;

namespace Licit.AuthService.Application.Features.CQRS.Auth.VerifyRegister;

public record VerifyRegisterCommandRequest(
    string TemporaryToken,
    string Code
) : IRequest<VerifyRegisterCommandResponse>;
