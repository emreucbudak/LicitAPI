using FluentValidation;

namespace Licit.AuthService.Application.Features.CQRS.Auth.RefreshToken;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommandRequest>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token boş olamaz.");
    }
}
