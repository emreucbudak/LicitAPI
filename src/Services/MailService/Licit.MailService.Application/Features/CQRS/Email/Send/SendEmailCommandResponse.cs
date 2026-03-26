namespace Licit.MailService.Application.Features.CQRS.Email.Send;

public record SendEmailCommandResponse(
    Guid Id,
    string To,
    string Subject,
    string Status,
    DateTime? SentAt,
    DateTime CreatedAt
);
