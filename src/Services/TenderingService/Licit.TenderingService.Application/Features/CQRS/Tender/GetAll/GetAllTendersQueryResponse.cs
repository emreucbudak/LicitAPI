namespace Licit.TenderingService.Application.Features.CQRS.Tender.GetAll;

public record GetAllTendersQueryResponse(
    List<TenderSummaryDto> Tenders
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
    DateTime CreatedAt,
    int RuleCount
);
