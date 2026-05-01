using FluentValidation;
using Licit.AuthService.Application.Features.CQRS.Auth.Commands.RefreshToken;

namespace Licit.AuthService.Application.Validators.Auth.RefreshToken;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommandRequest>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token boş olamaz.");
    }
}
