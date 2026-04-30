using FlashMediator;

namespace Licit.MailService.Application.Features.CQRS.Email.Queries.GetAll;

public record GetAllEmailsQueryRequest(
    int Page,
    int PageSize
) : IRequest<GetAllEmailsQueryResponse>;
