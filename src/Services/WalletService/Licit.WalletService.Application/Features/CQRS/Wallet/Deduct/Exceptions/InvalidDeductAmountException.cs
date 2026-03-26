using Licit.WalletService.Application.Exceptions;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Deduct.Exceptions;

public class InvalidDeductAmountException : BusinessRuleException
{
    public InvalidDeductAmountException()
        : base("Kesilecek tutar sıfırdan büyük olmalıdır.") { }
}
