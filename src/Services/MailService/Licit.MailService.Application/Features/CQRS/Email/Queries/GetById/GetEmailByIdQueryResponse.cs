namespace Licit.MailService.Application.Features.CQRS.Email.Queries.GetById;

public record GetEmailByIdQueryResponse(
    Guid Id,
    string To,
    string Subject,
    string Body,
    string Status,
    string? ErrorMessage,
    DateTime? SentAt,
    DateTime CreatedAt
);
