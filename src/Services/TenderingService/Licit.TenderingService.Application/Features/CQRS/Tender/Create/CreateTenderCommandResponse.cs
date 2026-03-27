namespace Licit.TenderingService.Application.Features.CQRS.Tender.Create;

public record CreateTenderCommandResponse(
    Guid Id,
    string Title,
    string Description,
    decimal StartingPrice,
    DateTime StartDate,
    DateTime EndDate,
    string Status,
    Guid CategoryId,
    DateTime CreatedAt
);
