using FlashMediator;
using Licit.TenderingService.Application.Interfaces;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.GetAll;

public class GetAllTendersQueryHandler(
    IUnitOfWork unitOfWork) : IRequestHandler<GetAllTendersQueryRequest, GetAllTendersQueryResponse>
{
    public async Task<GetAllTendersQueryResponse> Handle(GetAllTendersQueryRequest request, CancellationToken cancellationToken)
    {
        var totalCount = await unitOfWork.Tenders.GetCountAsync();
        var tenders = await unitOfWork.Tenders.GetAllAsync(request.Page, request.PageSize);

        var dtos = tenders.Select(t => new TenderSummaryDto(
            t.Id,
            t.Title,
            t.Description,
            t.StartingPrice,
            t.StartDate,
            t.EndDate,
            t.Status.ToString(),
            t.CreatedByUserId,
            t.CategoryId,
            t.Category.Name,
            t.CreatedAt,
            t.Rules.Count
        )).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return new GetAllTendersQueryResponse(
            dtos, totalCount, request.Page, request.PageSize,
            totalPages, request.Page < totalPages, request.Page > 1);
    }
}
