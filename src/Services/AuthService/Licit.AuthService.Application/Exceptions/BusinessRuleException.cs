namespace Licit.AuthService.Application.Exceptions;

public class BusinessRuleException : BaseException
{
    public BusinessRuleException(string message) : base(message, 422) { }
}
