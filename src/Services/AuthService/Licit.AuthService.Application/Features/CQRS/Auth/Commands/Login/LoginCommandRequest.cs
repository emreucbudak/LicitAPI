using FlashMediator;

namespace Licit.AuthService.Application.Features.CQRS.Auth.Commands.Login;

public record LoginCommandRequest(
    string Email,
    string Password
) : IRequest<LoginCommandResponse>;
