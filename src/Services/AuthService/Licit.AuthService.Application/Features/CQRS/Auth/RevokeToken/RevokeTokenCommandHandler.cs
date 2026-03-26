using FlashMediator;
using Licit.AuthService.Application.Interfaces;

namespace Licit.AuthService.Application.Features.CQRS.Auth.RevokeToken;

public class RevokeTokenCommandHandler(
    ITokenService tokenService) : IRequestHandler<RevokeTokenCommandRequest>
{
    public Task Handle(RevokeTokenCommandRequest request, CancellationToken cancellationToken)
    {
        var userId = tokenService.ValidateRefreshToken(request.RefreshToken)
            ?? throw new InvalidOperationException("Geçersiz token.");

        return Task.CompletedTask;
    }
}
