using Licit.TenderingService.Application.Exceptions;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.Commands.Delete.Exceptions;

public class ActiveTenderDeletionException : BusinessRuleException
{
    public ActiveTenderDeletionException()
        : base("Aktif bir ihale silinemez. Önce iptal edilmelidir.") { }
}
