using Licit.AuthService.Application.Exceptions;

namespace Licit.AuthService.Application.Features.CQRS.Auth.Register.Exceptions;

public class EmailAlreadyExistsException : ConflictException
{
    public EmailAlreadyExistsException()
        : base("Bu e-posta adresi zaten kayıtlı.") { }
}
