namespace Licit.TenderingService.Application.Features.CQRS.Tender.Commands.UploadImage;

public record UploadTenderImageCommandResponse(
    Guid Id,
    string ImageUrl,
    DateTime? UpdatedAt
);
