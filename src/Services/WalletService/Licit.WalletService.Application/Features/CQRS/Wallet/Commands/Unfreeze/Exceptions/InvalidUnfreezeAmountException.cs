using Licit.WalletService.Application.Exceptions;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Unfreeze.Exceptions;

public class InvalidUnfreezeAmountException : BusinessRuleException
{
    public InvalidUnfreezeAmountException()
        : base("Çözülecek tutar sıfırdan büyük olmalıdır.") { }
}
