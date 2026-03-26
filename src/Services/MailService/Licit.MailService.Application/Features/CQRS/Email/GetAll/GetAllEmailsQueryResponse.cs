namespace Licit.MailService.Application.Features.CQRS.Email.GetAll;

public record GetAllEmailsQueryResponse(
    List<EmailSummaryDto> Emails
);

public record EmailSummaryDto(
    Guid Id,
    string To,
    string Subject,
    string Status,
    DateTime? SentAt,
    DateTime CreatedAt
);
