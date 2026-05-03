namespace Licit.TenderingService.Application.Features.CQRS.Tender.Queries.GetById;

public record GetTenderByIdQueryResponse(
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
    DateTime? UpdatedAt
);
