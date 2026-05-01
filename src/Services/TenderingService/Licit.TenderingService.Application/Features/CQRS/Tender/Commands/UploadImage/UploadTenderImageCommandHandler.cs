using FlashMediator;
using FluentValidation;
using Licit.TenderingService.Application.Exceptions;
using Licit.TenderingService.Application.Features.CQRS.Tender.Queries.GetById.Exceptions;
using Licit.TenderingService.Application.Interfaces;
using Licit.TenderingService.Domain.Entities;
using Licit.TenderingService.Domain.Exceptions;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.Commands.UploadImage;

public class UploadTenderImageCommandHandler(
    IUnitOfWork unitOfWork,
    ITenderRepository tenderRepository,
    IValidator<UploadTenderImageCommandRequest> validator,
    ITenderImageStorageService imageStorage,
    ICurrentUserService currentUserService,
    ITenderCacheInvalidator cacheInvalidator) : IRequestHandler<UploadTenderImageCommandRequest, UploadTenderImageCommandResponse>
{
    public async Task<UploadTenderImageCommandResponse> Handle(UploadTenderImageCommandRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var tender = await tenderRepository.GetByIdAsync(request.Id)
            ?? throw new TenderNotFoundException(request.Id);

        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("Kullanici kimligi bulunamadi.");

        if (tender.CreatedByUserId != userId)
            throw new ForbiddenException("Bu ihalenin gorselini yalnizca sahibi guncelleyebilir.");

        if (tender.Status != TenderStatus.Draft)
            throw new TenderNotEditableException();

        var previousBlobName = tender.ImageBlobName;
        var uploadResult = await imageStorage.UploadTenderImageAsync(
            tender.Id,
            request.FileName,
            request.ContentType,
            request.ImageStream,
            cancellationToken);

        tender.SetImage(uploadResult.ImageUrl, uploadResult.BlobName);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cacheInvalidator.InvalidateAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(previousBlobName) &&
            !string.Equals(previousBlobName, uploadResult.BlobName, StringComparison.Ordinal))
        {
            await imageStorage.DeleteTenderImageAsync(previousBlobName, cancellationToken);
        }

        return new UploadTenderImageCommandResponse(tender.Id, tender.ImageUrl!, tender.UpdatedAt);
    }
}
