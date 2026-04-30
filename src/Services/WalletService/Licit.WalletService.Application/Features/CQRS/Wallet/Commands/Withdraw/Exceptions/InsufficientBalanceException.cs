using Licit.WalletService.Application.Exceptions;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Withdraw.Exceptions;

public class InsufficientBalanceException : BusinessRuleException
{
    public InsufficientBalanceException()
        : base("Yetersiz bakiye.") { }
}
