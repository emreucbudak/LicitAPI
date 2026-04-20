namespace Licit.MailService.Application.DTOs;

public class AzureCommunicationEmailSettings
{
    public string ConnectionString { get; set; } = null!;
    public string FromEmail { get; set; } = null!;
}
