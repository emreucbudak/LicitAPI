using FlashMediator;
using Licit.TenderingService.Application.Interfaces;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.GetAll;

public record GetAllTendersQueryRequest(
    int Page = 1,
    int PageSize = 20
) : IRequest<GetAllTendersQueryResponse>, ICacheableQuery;
