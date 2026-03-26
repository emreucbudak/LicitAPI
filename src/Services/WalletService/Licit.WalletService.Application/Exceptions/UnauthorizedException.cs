namespace Licit.WalletService.Application.Exceptions;

public class UnauthorizedException : BaseException
{
    public UnauthorizedException(string message) : base(message, 401) { }
}
