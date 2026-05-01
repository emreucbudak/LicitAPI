using FluentValidation;
using Licit.AuthService.Application.Features.CQRS.Auth.VerifyLogin;

namespace Licit.AuthService.Application.Validators.Auth.VerifyLogin;

public class VerifyLoginCommandValidator : AbstractValidator<VerifyLoginCommandRequest>
{
    public VerifyLoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta adresi boş olamaz.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Doğrulama kodu boş olamaz.")
            .Matches(@"^\d{6}$").WithMessage("Doğrulama kodu 6 haneli sayısal olmalıdır.");
    }
}
