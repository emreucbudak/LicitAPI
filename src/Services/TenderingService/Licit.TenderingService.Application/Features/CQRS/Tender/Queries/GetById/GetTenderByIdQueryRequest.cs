using FlashMediator;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.Queries.GetById;

public record GetTenderByIdQueryRequest(
    Guid Id
) : IRequest<GetTenderByIdQueryResponse>;
