using Licit.AuthService.Application.Exceptions;

namespace Licit.AuthService.Application.Features.CQRS.Auth.Login.Exceptions;

public class InvalidCredentialsException : UnauthorizedException
{
    public InvalidCredentialsException()
        : base("Geçersiz e-posta veya şifre.") { }
}
