using FlashMediator;
using FluentValidation;
using Licit.TenderingService.Application.Interfaces;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.Queries.GetAll;

public class GetAllTendersQueryHandler(
    ITenderRepository tenderRepository,
    IValidator<GetAllTendersQueryRequest> validator) : IRequestHandler<GetAllTendersQueryRequest, GetAllTendersQueryResponse>
{
    public async Task<GetAllTendersQueryResponse> Handle(GetAllTendersQueryRequest request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var hasFilters =
            !string.IsNullOrWhiteSpace(request.Search) ||
            request.ActiveOnly ||
            request.CategoryId.HasValue;
        var totalCount = hasFilters
            ? await tenderRepository.GetSearchCountAsync(request.Search, request.ActiveOnly, request.CategoryId)
            : await tenderRepository.GetCountAsync();
        var tenders = hasFilters
            ? await tenderRepository.SearchAsync(request.Search, request.ActiveOnly, request.CategoryId, request.Page, request.PageSize)
            : await tenderRepository.GetAllAsync(request.Page, request.PageSize);

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
            t.Category?.Name ?? string.Empty,
            t.ImageUrl,
            t.ImageUrls,
            t.CreatedAt
        )).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return new GetAllTendersQueryResponse(
            dtos, totalCount, request.Page, request.PageSize,
            totalPages, request.Page < totalPages, request.Page > 1);
    }
}
