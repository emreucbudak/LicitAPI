namespace Licit.MailService.Application.Exceptions;

public class BusinessRuleException : BaseException
{
    public BusinessRuleException(string message) : base(message, 422) { }
}
