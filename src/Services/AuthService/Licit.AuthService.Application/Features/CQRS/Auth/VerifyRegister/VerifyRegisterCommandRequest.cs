using FlashMediator;

namespace Licit.AuthService.Application.Features.CQRS.Auth.VerifyRegister;

public record VerifyRegisterCommandRequest(
    string Email,
    string Code
) : IRequest<VerifyRegisterCommandResponse>;
