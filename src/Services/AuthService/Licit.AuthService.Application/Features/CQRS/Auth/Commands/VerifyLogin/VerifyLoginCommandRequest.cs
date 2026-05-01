using FlashMediator;

namespace Licit.AuthService.Application.Features.CQRS.Auth.Commands.VerifyLogin;

public record VerifyLoginCommandRequest(
    string Email,
    string Code
) : IRequest<VerifyLoginCommandResponse>;
