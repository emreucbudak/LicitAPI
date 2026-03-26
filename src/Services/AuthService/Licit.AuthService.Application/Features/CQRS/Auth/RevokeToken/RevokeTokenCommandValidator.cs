using FluentValidation;

namespace Licit.AuthService.Application.Features.CQRS.Auth.RevokeToken;

public class RevokeTokenCommandValidator : AbstractValidator<RevokeTokenCommandRequest>
{
    public RevokeTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token boş olamaz.");
    }
}
