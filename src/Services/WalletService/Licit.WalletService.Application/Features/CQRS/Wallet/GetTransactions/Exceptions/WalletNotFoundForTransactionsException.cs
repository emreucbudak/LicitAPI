using Licit.WalletService.Application.Exceptions;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.GetTransactions.Exceptions;

public class WalletNotFoundForTransactionsException : NotFoundException
{
    public WalletNotFoundForTransactionsException(Guid userId)
        : base("Cüzdan", userId) { }
}
