namespace Licit.TenderingService.Application.Features.CQRS.Tender.ChangeStatus;

public record ChangeTenderStatusCommandResponse(
    Guid Id,
    string Status,
    DateTime? UpdatedAt
);
