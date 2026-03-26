using FlashMediator;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.GetAll;

public record GetAllTendersQueryRequest() : IRequest<GetAllTendersQueryResponse>;
