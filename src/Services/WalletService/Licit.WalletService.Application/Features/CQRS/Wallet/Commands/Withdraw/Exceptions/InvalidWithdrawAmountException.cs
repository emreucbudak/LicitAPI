using Licit.WalletService.Application.Exceptions;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Withdraw.Exceptions;

public class InvalidWithdrawAmountException : BusinessRuleException
{
    public InvalidWithdrawAmountException()
        : base("Çekilecek tutar sıfırdan büyük olmalıdır.") { }
}
