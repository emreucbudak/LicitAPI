using Licit.MailService.Application.Exceptions;

namespace Licit.MailService.Application.Features.CQRS.Email.GetById.Exceptions;

public class EmailNotFoundException : NotFoundException
{
    public EmailNotFoundException(Guid id)
        : base("E-posta", id) { }
}
