using Licit.WalletService.Application.Exceptions;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Unfreeze.Exceptions;

public class InsufficientFrozenBalanceException : BusinessRuleException
{
    public InsufficientFrozenBalanceException()
        : base("Bloke edilmiş bakiye yetersiz.") { }
}
