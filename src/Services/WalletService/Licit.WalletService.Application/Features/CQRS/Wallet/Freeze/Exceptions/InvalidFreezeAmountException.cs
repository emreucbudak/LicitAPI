using Licit.WalletService.Application.Exceptions;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Freeze.Exceptions;

public class InvalidFreezeAmountException : BusinessRuleException
{
    public InvalidFreezeAmountException()
        : base("Bloke edilecek tutar sıfırdan büyük olmalıdır.") { }
}
