namespace Licit.TenderingService.Application.Interfaces;

public interface ITenderImageStorageService
{
    Task<TenderImageUploadResult> UploadTenderImageAsync(
        Guid tenderId,
        string fileName,
        string contentType,
        Stream content,
        CancellationToken cancellationToken = default);

    Task DeleteTenderImageAsync(string blobName, CancellationToken cancellationToken = default);
}

public record TenderImageUploadResult(string ImageUrl, string BlobName);
