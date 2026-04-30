using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Licit.TenderingService.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace Licit.TenderingService.Infrastructure.Services;

public class AzureBlobTenderImageStorageService(IOptions<TenderImageStorageOptions> options) : ITenderImageStorageService
{
    private readonly TenderImageStorageOptions _options = options.Value;

    public async Task<TenderImageUploadResult> UploadTenderImageAsync(
        Guid tenderId,
        string fileName,
        string contentType,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ConnectionString))
            throw new InvalidOperationException("TenderImages:ConnectionString must be configured.");

        var containerName = string.IsNullOrWhiteSpace(_options.ContainerName)
            ? "tender-images"
            : _options.ContainerName.Trim();

        var containerClient = new BlobContainerClient(_options.ConnectionString, containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);

        var blobName = $"tenders/{tenderId:N}/{Guid.CreateVersion7():N}{GetExtension(fileName, contentType)}";
        var blobClient = containerClient.GetBlobClient(blobName);

        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType,
                CacheControl = "public, max-age=31536000"
            }
        };

        await blobClient.UploadAsync(content, uploadOptions, cancellationToken);

        return new TenderImageUploadResult(BuildPublicUrl(blobClient, blobName), blobName);
    }

    public async Task DeleteTenderImageAsync(string blobName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ConnectionString) || string.IsNullOrWhiteSpace(blobName))
            return;

        var containerName = string.IsNullOrWhiteSpace(_options.ContainerName)
            ? "tender-images"
            : _options.ContainerName.Trim();

        var blobClient = new BlobClient(_options.ConnectionString, containerName, blobName);
        await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
    }

    private string BuildPublicUrl(BlobClient blobClient, string blobName)
    {
        if (string.IsNullOrWhiteSpace(_options.PublicBaseUrl))
            return blobClient.Uri.ToString();

        return $"{_options.PublicBaseUrl.TrimEnd('/')}/{blobName}";
    }

    private static string GetExtension(string fileName, string contentType)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (extension is ".jpg" or ".jpeg" or ".png" or ".webp")
            return extension;

        return contentType.ToLowerInvariant() switch
        {
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => ".jpg"
        };
    }
}
