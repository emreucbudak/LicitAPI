using FluentValidation;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Unfreeze;

public class UnfreezeFundsCommandValidator : AbstractValidator<UnfreezeFundsCommandRequest>
{
    public UnfreezeFundsCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Kullanıcı kimliği belirtilmelidir.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Çözülecek tutar sıfırdan büyük olmalıdır.");
        RuleFor(x => x.ReferenceId).NotEmpty().WithMessage("Referans kimliği belirtilmelidir.");
    }
}
