using Licit.TenderingService.Application.Exceptions;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.Queries.GetById.Exceptions;

public class TenderNotFoundException : NotFoundException
{
    public TenderNotFoundException(Guid id)
        : base("İhale", id) { }
}
