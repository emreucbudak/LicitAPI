using FluentValidation;

namespace Licit.AuthService.Application.Features.CQRS.Auth.VerifyLogin;

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

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Geçici oturum bilgisi eksik.");

        RuleFor(x => x.TemporaryTokenEmail)
            .NotEmpty().WithMessage("Geçici oturum e-posta bilgisi eksik.")
            .EmailAddress().WithMessage("Geçici oturum e-posta bilgisi geçersiz.");

        RuleFor(x => x.TemporaryTokenId)
            .NotEmpty().WithMessage("Geçici oturum doğrulama bilgisi eksik.");
    }
}
