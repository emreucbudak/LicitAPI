namespace Licit.MailService.Application.Exceptions;

public class NotFoundException : BaseException
{
    public NotFoundException(string entity, object key)
        : base($"{entity} bulunamadı. Anahtar: {key}", 404) { }
}
