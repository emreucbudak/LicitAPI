using FluentValidation;
using Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Unfreeze;

namespace Licit.WalletService.Application.Validators.Wallet.Commands.Unfreeze;

public class UnfreezeFundsCommandValidator : AbstractValidator<UnfreezeFundsCommandRequest>
{
    public UnfreezeFundsCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Kullanıcı kimliği belirtilmelidir.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Çözülecek tutar sıfırdan büyük olmalıdır.");
        RuleFor(x => x.ReferenceId).NotEmpty().WithMessage("Referans kimliği belirtilmelidir.");
    }
}
