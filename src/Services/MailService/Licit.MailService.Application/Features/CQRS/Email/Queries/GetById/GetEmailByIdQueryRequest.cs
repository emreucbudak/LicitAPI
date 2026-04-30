using FlashMediator;

namespace Licit.MailService.Application.Features.CQRS.Email.Queries.GetById;

public record GetEmailByIdQueryRequest(
    Guid Id
) : IRequest<GetEmailByIdQueryResponse>;
