using FlashMediator;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.UploadImage;

public record UploadTenderImageCommandRequest(
    Guid Id,
    Guid UserId,
    Stream ImageStream,
    string FileName,
    string ContentType,
    long Length
) : IRequest<UploadTenderImageCommandResponse>;
