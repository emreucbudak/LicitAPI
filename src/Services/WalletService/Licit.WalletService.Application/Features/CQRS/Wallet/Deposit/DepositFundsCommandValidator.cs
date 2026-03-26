using FluentValidation;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Deposit;

public class DepositFundsCommandValidator : AbstractValidator<DepositFundsCommandRequest>
{
    public DepositFundsCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Kullanıcı kimliği belirtilmelidir.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Yatırılacak tutar sıfırdan büyük olmalıdır.");
    }
}
