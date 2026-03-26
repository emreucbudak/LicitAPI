using FlashMediator;
using Licit.TenderingService.Application.Interfaces;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.GetAll;

public class GetAllTendersQueryHandler(
    IUnitOfWork unitOfWork) : IRequestHandler<GetAllTendersQueryRequest, GetAllTendersQueryResponse>
{
    public async Task<GetAllTendersQueryResponse> Handle(GetAllTendersQueryRequest request, CancellationToken cancellationToken)
    {
        var tenders = await unitOfWork.Tenders.GetAllAsync();

        var dtos = tenders.Select(t => new TenderSummaryDto(
            t.Id,
            t.Title,
            t.Description,
            t.StartingPrice,
            t.StartDate,
            t.EndDate,
            t.Status.ToString(),
            t.CreatedByUserId,
            t.CreatedAt,
            t.Rules.Count
        )).ToList();

        return new GetAllTendersQueryResponse(dtos);
    }
}
