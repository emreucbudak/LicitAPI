using FluentValidation;

namespace Licit.AuthService.Application.Features.CQRS.Auth.VerifyForgotPassword;

public class VerifyForgotPasswordCommandValidator : AbstractValidator<VerifyForgotPasswordCommandRequest>
{
    public VerifyForgotPasswordCommandValidator()
    {
        RuleFor(x => x.TemporaryToken)
            .NotEmpty().WithMessage("Gecici sifre sifirlama tokeni bos olamaz.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Dogrulama kodu bos olamaz.")
            .Length(6).WithMessage("Dogrulama kodu 6 haneli olmalidir.");
    }
}
