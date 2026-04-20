namespace Licit.AuthService.Application.Exceptions;

public class PasswordReuseNotAllowedException : BusinessRuleException
{
    public PasswordReuseNotAllowedException()
        : base("Yeni sifre son 3 sifreden biriyle ayni olamaz.")
    {
    }
}
