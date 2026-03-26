using Licit.WalletService.Application.Exceptions;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Withdraw.Exceptions;

public class InsufficientBalanceException : BusinessRuleException
{
    public InsufficientBalanceException()
        : base("Yetersiz bakiye.") { }
}
