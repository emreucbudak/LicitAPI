using FlashMediator;
using FluentValidation;
using Licit.MailService.Application.Features.CQRS.Email.Queries.GetById.Exceptions;
using Licit.MailService.Application.Interfaces;

namespace Licit.MailService.Application.Features.CQRS.Email.Queries.GetById;

public class GetEmailByIdQueryHandler(
    IEmailRepository emailRepository,
    IValidator<GetEmailByIdQueryRequest> validator) : IRequestHandler<GetEmailByIdQueryRequest, GetEmailByIdQueryResponse>
{
    public async Task<GetEmailByIdQueryResponse> Handle(GetEmailByIdQueryRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var email = await emailRepository.GetByIdAsync(request.Id)
            ?? throw new EmailNotFoundException(request.Id);

        return new GetEmailByIdQueryResponse(
            email.Id,
            email.To,
            email.Subject,
            email.Body,
            email.Status.ToString(),
            email.ErrorMessage,
            email.SentAt,
            email.CreatedAt
        );
    }
}
