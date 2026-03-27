namespace Licit.TenderingService.Application.Features.CQRS.Tender.Update;

public record UpdateTenderCommandResponse(
    Guid Id,
    string Title,
    string Description,
    decimal StartingPrice,
    DateTime StartDate,
    DateTime EndDate,
    string Status,
    Guid CategoryId,
    DateTime? UpdatedAt
);
