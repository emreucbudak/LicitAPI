using FlashMediator;
using FluentValidation;
using Licit.TenderingService.Application.Features.CQRS.Tender.Commands.ChangeStatus.Exceptions;
using Licit.TenderingService.Application.Features.CQRS.Tender.Queries.GetById.Exceptions;
using Licit.TenderingService.Application.Interfaces;
using Licit.TenderingService.Domain.Entities;
using Microsoft.Extensions.Logging;
using DomainExceptions = Licit.TenderingService.Domain.Exceptions;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.Commands.ChangeStatus;

public class ChangeTenderStatusCommandHandler(
    IUnitOfWork unitOfWork,
    ITenderRepository tenderRepository,
    IValidator<ChangeTenderStatusCommandRequest> validator,
    ITenderCacheInvalidator cacheInvalidator,
    IEventPublisher eventPublisher,
    ILogger<ChangeTenderStatusCommandHandler> logger) : IRequestHandler<ChangeTenderStatusCommandRequest, ChangeTenderStatusCommandResponse>
{
    public async Task<ChangeTenderStatusCommandResponse> Handle(ChangeTenderStatusCommandRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var tender = await tenderRepository.GetByIdAsync(request.Id)
            ?? throw new TenderNotFoundException(request.Id);

        if (!Enum.TryParse<TenderStatus>(request.Status, true, out var newStatus))
            throw new InvalidTenderStatusException(request.Status);

        try
        {
            tender.ChangeStatus(newStatus);
        }
        catch (DomainExceptions.InvalidStatusTransitionException ex)
        {
            throw new InvalidStatusTransitionException(ex.FromStatus, ex.ToStatus);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cacheInvalidator.InvalidateAsync(cancellationToken);
        try
        {
            await eventPublisher.PublishTenderStatusChangedAsync(tender.Id, tender.Title, tender.Status.ToString(), tender.ImageUrl, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Tender status was saved but TenderStatusChanged event could not be published. TenderId: {TenderId}, Status: {Status}",
                tender.Id,
                tender.Status);
        }

        return new ChangeTenderStatusCommandResponse(tender.Id, tender.Status.ToString(), tender.UpdatedAt);
    }
}
