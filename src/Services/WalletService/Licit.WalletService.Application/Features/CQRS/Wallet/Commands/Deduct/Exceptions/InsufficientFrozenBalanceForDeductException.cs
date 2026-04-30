using Licit.WalletService.Application.Exceptions;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Deduct.Exceptions;

public class InsufficientFrozenBalanceForDeductException : BusinessRuleException
{
    public InsufficientFrozenBalanceForDeductException()
        : base("Bloke edilmiş bakiye yetersiz. Kesim yapılamaz.") { }
}
