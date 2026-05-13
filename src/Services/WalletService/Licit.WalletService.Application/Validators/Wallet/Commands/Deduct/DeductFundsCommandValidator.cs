using FluentValidation;
using Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Deduct;

namespace Licit.WalletService.Application.Validators.Wallet.Commands.Deduct;

public class DeductFundsCommandValidator : AbstractValidator<DeductFundsCommandRequest>
{
    public DeductFundsCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Kullanıcı kimliği belirtilmelidir.");
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Kesilecek tutar sıfırdan büyük olmalıdır.")
            .Must(amount => decimal.Truncate(amount) == amount)
            .WithMessage("Kesilecek tutar tam TL olmalıdır.");
        RuleFor(x => x.ReferenceId).NotEmpty().WithMessage("Referans kimliği belirtilmelidir.");
    }
}
