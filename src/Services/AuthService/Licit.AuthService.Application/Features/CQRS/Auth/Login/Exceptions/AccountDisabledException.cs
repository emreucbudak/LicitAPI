using Licit.AuthService.Application.Exceptions;

namespace Licit.AuthService.Application.Features.CQRS.Auth.Login.Exceptions;

public class AccountDisabledException : UnauthorizedException
{
    public AccountDisabledException()
        : base("Hesap devre dışı bırakılmış.") { }
}
