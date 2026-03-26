using FlashMediator;

namespace Licit.AuthService.Application.Features.CQRS.Auth.RevokeToken;

public record RevokeTokenCommandRequest(
    string RefreshToken
) : IRequest;
