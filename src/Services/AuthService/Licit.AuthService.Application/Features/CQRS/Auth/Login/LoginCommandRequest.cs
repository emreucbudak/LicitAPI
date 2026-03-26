using FlashMediator;

namespace Licit.AuthService.Application.Features.CQRS.Auth.Login;

public record LoginCommandRequest(
    string Email,
    string Password
) : IRequest<LoginCommandResponse>;
