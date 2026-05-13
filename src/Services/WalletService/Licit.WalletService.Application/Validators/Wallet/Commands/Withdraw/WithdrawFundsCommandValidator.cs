using FluentValidation;
using Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Withdraw;

namespace Licit.WalletService.Application.Validators.Wallet.Commands.Withdraw;

public class WithdrawFundsCommandValidator : AbstractValidator<WithdrawFundsCommandRequest>
{
    public WithdrawFundsCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Kullanıcı kimliği belirtilmelidir.");
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Çekilecek tutar sıfırdan büyük olmalıdır.")
            .Must(amount => decimal.Truncate(amount) == amount)
            .WithMessage("Çekilecek tutar tam TL olmalıdır.");
    }
}
