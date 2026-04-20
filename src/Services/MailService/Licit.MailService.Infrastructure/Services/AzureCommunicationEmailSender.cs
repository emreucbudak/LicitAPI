using Azure;
using Azure.Communication.Email;
using Licit.MailService.Application.DTOs;
using Licit.MailService.Application.Interfaces;

namespace Licit.MailService.Infrastructure.Services;

public class AzureCommunicationEmailSender : IEmailSender
{
    private readonly EmailClient _emailClient;
    private readonly AzureCommunicationEmailSettings _settings;

    public AzureCommunicationEmailSender(
        EmailClient emailClient,
        AzureCommunicationEmailSettings settings)
    {
        _emailClient = emailClient;
        _settings = settings;
    }

    public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        var content = new EmailContent(subject)
        {
            Html = body
        };

        var message = new EmailMessage(
            senderAddress: _settings.FromEmail,
            recipientAddress: to,
            content: content);

        var operation = await _emailClient.SendAsync(WaitUntil.Completed, message, cancellationToken);
        var status = operation.Value.Status.ToString();

        if (!string.Equals(status, "Succeeded", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Azure Communication Services email send failed with status '{status}'.");
    }
}
