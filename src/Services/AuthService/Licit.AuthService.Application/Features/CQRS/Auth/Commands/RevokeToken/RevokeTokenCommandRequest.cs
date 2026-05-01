using FlashMediator;

namespace Licit.AuthService.Application.Features.CQRS.Auth.Commands.RevokeToken;

public record RevokeTokenCommandRequest(
    string RefreshToken
) : IRequest;
