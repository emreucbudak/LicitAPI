using FlashMediator;
using FluentValidation;
using Licit.TenderingService.Application.Exceptions;
using Licit.TenderingService.Application.Features.CQRS.Tender.Queries.GetById.Exceptions;
using Licit.TenderingService.Application.Interfaces;
using Licit.TenderingService.Domain.Entities;
using Licit.TenderingService.Domain.Exceptions;
using TenderEntity = Licit.TenderingService.Domain.Entities.Tender;

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

        if (!request.ReplaceExisting && tender.ImageUrls.Length >= TenderEntity.MaxImageCount)
            throw new InvalidOperationException($"Bir ihaleye en fazla {TenderEntity.MaxImageCount} gorsel yuklenebilir.");

        var previousBlobNames = request.ReplaceExisting
            ? GetExistingBlobNames(tender)
            : [];

        var uploadResult = await imageStorage.UploadTenderImageAsync(
            tender.Id,
            request.FileName,
            request.ContentType,
            request.ImageStream,
            cancellationToken);

        if (request.ReplaceExisting)
            tender.SetImage(uploadResult.ImageUrl, uploadResult.BlobName);
        else
            tender.AddImage(uploadResult.ImageUrl, uploadResult.BlobName);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cacheInvalidator.InvalidateAsync(cancellationToken);

        foreach (var previousBlobName in previousBlobNames)
        {
            if (!string.Equals(previousBlobName, uploadResult.BlobName, StringComparison.Ordinal))
                await imageStorage.DeleteTenderImageAsync(previousBlobName, cancellationToken);
        }

        return new UploadTenderImageCommandResponse(tender.Id, tender.ImageUrl!, tender.ImageUrls, tender.UpdatedAt);
    }

    private static string[] GetExistingBlobNames(TenderEntity tender)
    {
        if (tender.ImageBlobNames.Length > 0)
            return tender.ImageBlobNames
                .Where(blobName => !string.IsNullOrWhiteSpace(blobName))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

        return string.IsNullOrWhiteSpace(tender.ImageBlobName)
            ? []
            : [tender.ImageBlobName];
    }
}
