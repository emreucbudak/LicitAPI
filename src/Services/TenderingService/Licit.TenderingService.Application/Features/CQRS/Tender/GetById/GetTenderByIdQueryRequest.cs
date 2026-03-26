using FlashMediator;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.GetById;

public record GetTenderByIdQueryRequest(
    Guid Id
) : IRequest<GetTenderByIdQueryResponse>;
