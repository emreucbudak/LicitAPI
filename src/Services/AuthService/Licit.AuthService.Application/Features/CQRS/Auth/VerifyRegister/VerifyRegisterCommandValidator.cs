using FluentValidation;

namespace Licit.AuthService.Application.Features.CQRS.Auth.VerifyRegister;

public class VerifyRegisterCommandValidator : AbstractValidator<VerifyRegisterCommandRequest>
{
    public VerifyRegisterCommandValidator()
    {
        RuleFor(x => x.TemporaryToken)
            .NotEmpty().WithMessage("Gecici dogrulama tokeni bos olamaz.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Dogrulama kodu bos olamaz.")
            .Length(6).WithMessage("Dogrulama kodu 6 haneli olmalidir.");
    }
}
