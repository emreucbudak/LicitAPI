using Licit.WalletService.Application.Exceptions;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Withdraw.Exceptions;

public class WalletNotFoundException : NotFoundException
{
    public WalletNotFoundException(Guid userId)
        : base("Cüzdan", userId) { }
}
