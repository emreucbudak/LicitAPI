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
        var connectionString = RequireOption(_options.ConnectionString, "TenderImages:ConnectionString");
        var containerName = RequireOption(_options.ContainerName, "TenderImages:ContainerName").Trim();

        var containerClient = new BlobContainerClient(connectionString, containerName);
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

        return new TenderImageUploadResult(BuildPublicUrl(blobName), blobName);
    }

    public async Task DeleteTenderImageAsync(string blobName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(blobName))
            return;

        var connectionString = RequireOption(_options.ConnectionString, "TenderImages:ConnectionString");
        var containerName = RequireOption(_options.ContainerName, "TenderImages:ContainerName").Trim();

        var blobClient = new BlobClient(connectionString, containerName, blobName);
        await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
    }

    private string BuildPublicUrl(string blobName)
    {
        var publicBaseUrl = RequireOption(_options.PublicBaseUrl, "TenderImages:PublicBaseUrl");
        return $"{publicBaseUrl.TrimEnd('/')}/{blobName}";
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

    private static string RequireOption(string? value, string key)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"{key} must be configured.");

        return value;
    }
}
