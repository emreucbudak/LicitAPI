using FlashMediator;

namespace Licit.AuthService.Application.Features.CQRS.Auth.Commands.RefreshToken;

public record RefreshTokenCommandRequest(
    string RefreshToken
) : IRequest<RefreshTokenCommandResponse>;
