using Licit.AuthService.Application.Exceptions;

namespace Licit.AuthService.Application.Features.CQRS.Auth.RevokeToken.Exceptions;

public class InvalidTokenException : BusinessRuleException
{
    public InvalidTokenException()
        : base("Geçersiz token.") { }
}
