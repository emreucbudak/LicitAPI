using Licit.WalletService.Application.Exceptions;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Deposit.Exceptions;

public class InvalidDepositAmountException : BusinessRuleException
{
    public InvalidDepositAmountException()
        : base("Yatırılacak tutar sıfırdan büyük olmalıdır.") { }
}
