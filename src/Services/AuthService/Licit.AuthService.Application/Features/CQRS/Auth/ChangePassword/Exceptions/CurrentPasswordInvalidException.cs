using Licit.AuthService.Application.Exceptions;

namespace Licit.AuthService.Application.Features.CQRS.Auth.ChangePassword.Exceptions;

public class CurrentPasswordInvalidException : BusinessRuleException
{
    public CurrentPasswordInvalidException() : base("Mevcut sifre hatali.")
    {
    }
}
