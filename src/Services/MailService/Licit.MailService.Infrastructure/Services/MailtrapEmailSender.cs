using Licit.MailService.Application.DTOs;
using Licit.MailService.Application.Interfaces;
using MailKit.Net.Smtp;
using MimeKit;

namespace Licit.MailService.Infrastructure.Services;

public class MailtrapEmailSender : IEmailSender
{
    private readonly SmtpSettings _settings;

    public MailtrapEmailSender(SmtpSettings settings)
    {
        _settings = settings;
    }

    public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        message.Body = new TextPart("html")
        {
            Text = body
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.Host, _settings.Port, MailKit.Security.SecureSocketOptions.StartTls, cancellationToken);
        await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
