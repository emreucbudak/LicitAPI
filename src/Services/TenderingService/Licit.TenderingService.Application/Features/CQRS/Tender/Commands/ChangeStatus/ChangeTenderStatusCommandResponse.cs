namespace Licit.TenderingService.Application.Features.CQRS.Tender.Commands.ChangeStatus;

public record ChangeTenderStatusCommandResponse(
    Guid Id,
    string Status,
    DateTime? UpdatedAt
);
