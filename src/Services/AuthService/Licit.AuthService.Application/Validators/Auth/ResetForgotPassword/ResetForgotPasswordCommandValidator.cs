using FluentValidation;
using Licit.AuthService.Application.Features.CQRS.Auth.Commands.ResetForgotPassword;

namespace Licit.AuthService.Application.Validators.Auth.ResetForgotPassword;

public class ResetForgotPasswordCommandValidator : AbstractValidator<ResetForgotPasswordCommandRequest>
{
    public ResetForgotPasswordCommandValidator()
    {
        RuleFor(x => x.TemporaryToken)
            .NotEmpty().WithMessage("Gecici sifre sifirlama tokeni bos olamaz.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Yeni sifre bos olamaz.")
            .MinimumLength(8).WithMessage("Yeni sifre en az 8 karakter olmali.");
    }
}
