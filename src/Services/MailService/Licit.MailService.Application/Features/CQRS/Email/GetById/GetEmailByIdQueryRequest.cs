using FlashMediator;

namespace Licit.MailService.Application.Features.CQRS.Email.GetById;

public record GetEmailByIdQueryRequest(
    Guid Id
) : IRequest<GetEmailByIdQueryResponse>;
