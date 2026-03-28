namespace Licit.MailService.Application.Features.CQRS.Email.GetAll;

public record GetAllEmailsQueryResponse(
    List<EmailSummaryDto> Emails,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage
);

public record EmailSummaryDto(
    Guid Id,
    string To,
    string Subject,
    string Status,
    DateTime? SentAt,
    DateTime CreatedAt
);
