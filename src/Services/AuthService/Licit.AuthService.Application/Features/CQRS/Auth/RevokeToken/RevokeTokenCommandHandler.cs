using FlashMediator;
using FluentValidation;
using Licit.AuthService.Application.Features.CQRS.Auth.RevokeToken.Exceptions;
using Licit.AuthService.Application.Interfaces;

namespace Licit.AuthService.Application.Features.CQRS.Auth.RevokeToken;

public class RevokeTokenCommandHandler(
    ITokenService tokenService,
    IValidator<RevokeTokenCommandRequest> validator) : IRequestHandler<RevokeTokenCommandRequest>
{
    public async Task Handle(RevokeTokenCommandRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        _ = tokenService.ValidateRefreshToken(request.RefreshToken)
            ?? throw new InvalidTokenException();
    }
}
