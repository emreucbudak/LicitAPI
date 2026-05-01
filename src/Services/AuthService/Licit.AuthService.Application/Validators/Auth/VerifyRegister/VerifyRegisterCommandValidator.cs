using FluentValidation;
using Licit.AuthService.Application.Features.CQRS.Auth.VerifyRegister;

namespace Licit.AuthService.Application.Validators.Auth.VerifyRegister;

public class VerifyRegisterCommandValidator : AbstractValidator<VerifyRegisterCommandRequest>
{
    public VerifyRegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta adresi bos olamaz.")
            .EmailAddress().WithMessage("Gecerli bir e-posta adresi giriniz.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Dogrulama kodu bos olamaz.")
            .Length(6).WithMessage("Dogrulama kodu 6 haneli olmalidir.");
    }
}
