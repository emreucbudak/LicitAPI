using FlashMediator;

namespace Licit.AuthService.Application.Features.CQRS.Auth.RefreshToken;

public record RefreshTokenCommandRequest(
    string RefreshToken
) : IRequest<RefreshTokenCommandResponse>;
