using Licit.AuthService.Application.Exceptions;

namespace Licit.AuthService.Application.Features.CQRS.Auth.RefreshToken.Exceptions;

public class InvalidRefreshTokenException : UnauthorizedException
{
    public InvalidRefreshTokenException()
        : base("Geçersiz veya süresi dolmuş refresh token.") { }
}
