using FluentValidation;

namespace Licit.AuthService.Application.Features.CQRS.Auth.ForgotPassword;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommandRequest>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta adresi bos olamaz.")
            .EmailAddress().WithMessage("Gecerli bir e-posta adresi giriniz.");
    }
}
