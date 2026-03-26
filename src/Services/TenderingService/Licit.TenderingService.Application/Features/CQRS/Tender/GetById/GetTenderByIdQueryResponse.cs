namespace Licit.TenderingService.Application.Features.CQRS.Tender.GetById;

public record GetTenderByIdQueryResponse(
    Guid Id,
    string Title,
    string Description,
    decimal StartingPrice,
    DateTime StartDate,
    DateTime EndDate,
    string Status,
    Guid CreatedByUserId,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<TenderRuleDto> Rules
);

public record TenderRuleDto(
    Guid Id,
    string Title,
    string Description,
    bool IsRequired
);
