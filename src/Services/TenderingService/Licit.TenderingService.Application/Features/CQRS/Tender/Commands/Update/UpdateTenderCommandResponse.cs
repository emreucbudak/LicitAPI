namespace Licit.TenderingService.Application.Features.CQRS.Tender.Commands.Update;

public record UpdateTenderCommandResponse(
    Guid Id,
    string Title,
    string Description,
    decimal StartingPrice,
    DateTime StartDate,
    DateTime EndDate,
    string Status,
    Guid CategoryId,
    string? ImageUrl,
    DateTime? UpdatedAt
);
