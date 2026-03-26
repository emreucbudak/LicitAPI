using FluentValidation;

namespace Licit.AuthService.Application.Features.CQRS.Auth.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommandRequest>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta adresi boş olamaz.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre boş olamaz.")
            .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ad boş olamaz.")
            .MaximumLength(100).WithMessage("Ad en fazla 100 karakter olabilir.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Soyad boş olamaz.")
            .MaximumLength(100).WithMessage("Soyad en fazla 100 karakter olabilir.");
    }
}
