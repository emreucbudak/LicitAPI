using FlashMediator;

namespace Licit.MailService.Application.Features.CQRS.Email.GetAll;

public record GetAllEmailsQueryRequest(
    int Page,
    int PageSize
) : IRequest<GetAllEmailsQueryResponse>;
