using Licit.AuthService.Application.Exceptions;

namespace Licit.AuthService.Application.Features.CQRS.Auth.VerifyLogin.Exceptions;

public class InvalidVerificationCodeException : UnauthorizedException
{
    public InvalidVerificationCodeException()
        : base("Doğrulama kodu veya geçici giriş bilgisi geçersiz.") { }
}
