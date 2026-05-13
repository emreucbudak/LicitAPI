namespace Licit.TenderingService.Application.Features.CQRS.Tender.Commands.Create;

public record CreateTenderCommandResponse(
    Guid Id,
    string Title,
    string Description,
    decimal StartingPrice,
    DateTime StartDate,
    DateTime EndDate,
    string Status,
    Guid CategoryId,
    string? ImageUrl,
    string[] ImageUrls,
    DateTime CreatedAt
);
