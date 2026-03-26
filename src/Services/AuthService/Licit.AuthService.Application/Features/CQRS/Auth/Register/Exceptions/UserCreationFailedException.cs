using Licit.AuthService.Application.Exceptions;

namespace Licit.AuthService.Application.Features.CQRS.Auth.Register.Exceptions;

public class UserCreationFailedException : BusinessRuleException
{
    public UserCreationFailedException(string errors)
        : base($"Kullanıcı oluşturulamadı: {errors}") { }
}
