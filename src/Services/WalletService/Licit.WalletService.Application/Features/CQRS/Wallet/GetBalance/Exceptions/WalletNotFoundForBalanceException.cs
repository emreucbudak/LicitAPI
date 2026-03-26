using Licit.WalletService.Application.Exceptions;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.GetBalance.Exceptions;

public class WalletNotFoundForBalanceException : NotFoundException
{
    public WalletNotFoundForBalanceException(Guid userId)
        : base("Cüzdan", userId) { }
}
