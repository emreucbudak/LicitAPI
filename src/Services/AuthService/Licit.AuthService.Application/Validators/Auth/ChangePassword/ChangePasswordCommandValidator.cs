using FluentValidation;
using Licit.AuthService.Application.Features.CQRS.Auth.ChangePassword;

namespace Licit.AuthService.Application.Validators.Auth.ChangePassword;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommandRequest>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Mevcut sifre bos olamaz.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Yeni sifre bos olamaz.")
            .MinimumLength(8).WithMessage("Yeni sifre en az 8 karakter olmali.");
    }
}
