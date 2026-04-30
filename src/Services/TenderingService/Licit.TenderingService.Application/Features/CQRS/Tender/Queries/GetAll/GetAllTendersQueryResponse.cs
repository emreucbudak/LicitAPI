namespace Licit.TenderingService.Application.Features.CQRS.Tender.Queries.GetAll;

public record GetAllTendersQueryResponse(
    List<TenderSummaryDto> Tenders,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage
);

public record TenderSummaryDto(
    Guid Id,
    string Title,
    string Description,
    decimal StartingPrice,
    DateTime StartDate,
    DateTime EndDate,
    string Status,
    Guid CreatedByUserId,
    Guid CategoryId,
    string CategoryName,
    string? ImageUrl,
    DateTime CreatedAt,
    int RuleCount
);
