namespace Licit.TenderingService.Infrastructure.Services;

public class TenderImageStorageOptions
{
    public string? ConnectionString { get; set; }
    public string ContainerName { get; set; } = "tender-images";
    public string? PublicBaseUrl { get; set; }
}
