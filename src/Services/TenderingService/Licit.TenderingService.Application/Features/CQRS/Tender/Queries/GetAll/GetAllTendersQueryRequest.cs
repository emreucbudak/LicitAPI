using FlashMediator;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.Queries.GetAll;

public record GetAllTendersQueryRequest(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    bool ActiveOnly = false,
    Guid? CategoryId = null
) : IRequest<GetAllTendersQueryResponse>;
