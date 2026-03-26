using FluentValidation;

namespace Licit.MailService.Application.Features.CQRS.Email.Send;

public class SendEmailCommandValidator : AbstractValidator<SendEmailCommandRequest>
{
    public SendEmailCommandValidator()
    {
        RuleFor(x => x.To)
            .NotEmpty().WithMessage("Alıcı e-posta adresi boş olamaz.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Konu boş olamaz.")
            .MaximumLength(500).WithMessage("Konu en fazla 500 karakter olabilir.");

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("E-posta içeriği boş olamaz.");
    }
}
