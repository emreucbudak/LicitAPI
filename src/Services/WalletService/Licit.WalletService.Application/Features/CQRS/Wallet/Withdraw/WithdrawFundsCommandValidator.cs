using FluentValidation;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Withdraw;

public class WithdrawFundsCommandValidator : AbstractValidator<WithdrawFundsCommandRequest>
{
    public WithdrawFundsCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Kullanıcı kimliği belirtilmelidir.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Çekilecek tutar sıfırdan büyük olmalıdır.");
    }
}
