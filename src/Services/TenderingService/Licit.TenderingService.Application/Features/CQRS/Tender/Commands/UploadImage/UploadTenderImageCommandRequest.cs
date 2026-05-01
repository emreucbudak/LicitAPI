using FlashMediator;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.Commands.UploadImage;

public record UploadTenderImageCommandRequest(
    Guid Id,
    Stream ImageStream,
    string FileName,
    string ContentType,
    long Length
) : IRequest<UploadTenderImageCommandResponse>;
