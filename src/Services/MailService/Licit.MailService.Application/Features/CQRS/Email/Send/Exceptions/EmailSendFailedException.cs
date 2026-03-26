using Licit.MailService.Application.Exceptions;

namespace Licit.MailService.Application.Features.CQRS.Email.Send.Exceptions;

public class EmailSendFailedException : BusinessRuleException
{
    public EmailSendFailedException(string error)
        : base($"E-posta gönderilemedi: {error}") { }
}
