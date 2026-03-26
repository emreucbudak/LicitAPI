using FlashMediator;
using FluentValidation;
using Licit.MailService.Application.Features.CQRS.Email.Send.Exceptions;
using Licit.MailService.Application.Interfaces;
using Licit.MailService.Domain.Entities;

namespace Licit.MailService.Application.Features.CQRS.Email.Send;

public class SendEmailCommandHandler(
    IUnitOfWork unitOfWork,
    IEmailSender emailSender,
    IValidator<SendEmailCommandRequest> validator) : IRequestHandler<SendEmailCommandRequest, SendEmailCommandResponse>
{
    public async Task<SendEmailCommandResponse> Handle(SendEmailCommandRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var email = new EmailMessage(request.To, request.Subject, request.Body);
        unitOfWork.Emails.Add(email);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            await emailSender.SendAsync(request.To, request.Subject, request.Body, cancellationToken);
            email.MarkAsSent();
        }
        catch (Exception ex)
        {
            email.MarkAsFailed(ex.Message);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (email.Status == EmailStatus.Failed)
            throw new EmailSendFailedException(email.ErrorMessage!);

        return new SendEmailCommandResponse(
            email.Id,
            email.To,
            email.Subject,
            email.Status.ToString(),
            email.SentAt,
            email.CreatedAt
        );
    }
}
