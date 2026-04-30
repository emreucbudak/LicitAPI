using Licit.WalletService.Application.Exceptions;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Freeze.Exceptions;

public class InsufficientBalanceForFreezeException : BusinessRuleException
{
    public InsufficientBalanceForFreezeException()
        : base("Yetersiz bakiye. Teklif vermek için yeterli kullanılabilir bakiye yok.") { }
}
