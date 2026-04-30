using Licit.WalletService.Application.Exceptions;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Deposit.Exceptions;

public class DuplicateDepositRequestException : ConflictException
{
    public DuplicateDepositRequestException()
        : base("Bu bakiye yukleme istegi zaten islenmis veya isleniyor.")
    {
    }
}
