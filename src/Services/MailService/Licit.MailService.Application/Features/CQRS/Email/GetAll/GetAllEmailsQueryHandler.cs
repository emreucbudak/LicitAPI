using FlashMediator;
using FluentValidation;
using Licit.MailService.Application.Interfaces;

namespace Licit.MailService.Application.Features.CQRS.Email.GetAll;

public class GetAllEmailsQueryHandler(
    IUnitOfWork unitOfWork,
    IValidator<GetAllEmailsQueryRequest> validator) : IRequestHandler<GetAllEmailsQueryRequest, GetAllEmailsQueryResponse>
{
    public async Task<GetAllEmailsQueryResponse> Handle(GetAllEmailsQueryRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var emails = await unitOfWork.Emails.GetAllAsync(request.Page, request.PageSize);

        var dtos = emails.Select(e => new EmailSummaryDto(
            e.Id,
            e.To,
            e.Subject,
            e.Status.ToString(),
            e.SentAt,
            e.CreatedAt
        )).ToList();

        return new GetAllEmailsQueryResponse(dtos);
    }
}
