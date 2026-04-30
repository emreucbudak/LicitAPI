namespace Licit.TenderingService.Application.Features.CQRS.Tender.UploadImage;

public record UploadTenderImageCommandResponse(
    Guid Id,
    string ImageUrl,
    DateTime? UpdatedAt
);
