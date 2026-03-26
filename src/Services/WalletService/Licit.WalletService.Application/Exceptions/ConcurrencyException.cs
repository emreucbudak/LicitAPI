namespace Licit.WalletService.Application.Exceptions;

public class ConcurrencyException : ConflictException
{
    public ConcurrencyException()
        : base("Eşzamanlılık hatası. Lütfen tekrar deneyin.") { }
}
