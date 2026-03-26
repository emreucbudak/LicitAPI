using FlashMediator;

namespace Licit.MailService.Application.Features.CQRS.Email.Send;

public record SendEmailCommandRequest(
    string To,
    string Subject,
    string Body
) : IRequest<SendEmailCommandResponse>;
