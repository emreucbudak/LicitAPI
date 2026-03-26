using Licit.MailService.Domain.Common;

namespace Licit.MailService.Domain.Entities;

public class EmailMessage : BaseEntity
{
    public string To { get; private set; } = null!;
    public string Subject { get; private set; } = null!;
    public string Body { get; private set; } = null!;
    public EmailStatus Status { get; private set; } = EmailStatus.Pending;
    public string? ErrorMessage { get; private set; }
    public DateTime? SentAt { get; private set; }

    private EmailMessage() { }

    public EmailMessage(string to, string subject, string body)
    {
        To = to;
        Subject = subject;
        Body = body;
        Status = EmailStatus.Pending;
    }

    public void MarkAsSent()
    {
        Status = EmailStatus.Sent;
        SentAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string error)
    {
        Status = EmailStatus.Failed;
        ErrorMessage = error;
        UpdatedAt = DateTime.UtcNow;
    }
}
