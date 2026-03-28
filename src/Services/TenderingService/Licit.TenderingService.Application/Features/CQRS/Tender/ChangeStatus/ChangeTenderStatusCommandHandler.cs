using FlashMediator;
using FluentValidation;
using Licit.TenderingService.Application.Features.CQRS.Tender.ChangeStatus.Exceptions;
using Licit.TenderingService.Application.Features.CQRS.Tender.GetById.Exceptions;
using Licit.TenderingService.Application.Interfaces;
using Licit.TenderingService.Domain.Entities;
using DomainExceptions = Licit.TenderingService.Domain.Exceptions;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.ChangeStatus;

public class ChangeTenderStatusCommandHandler(
    IUnitOfWork unitOfWork,
    IValidator<ChangeTenderStatusCommandRequest> validator,
    ITenderCacheInvalidator cacheInvalidator,
    IEventPublisher eventPublisher) : IRequestHandler<ChangeTenderStatusCommandRequest, ChangeTenderStatusCommandResponse>
{
    public async Task<ChangeTenderStatusCommandResponse> Handle(ChangeTenderStatusCommandRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var tender = await unitOfWork.Tenders.GetByIdAsync(request.Id)
            ?? throw new TenderNotFoundException(request.Id);

        if (!Enum.TryParse<TenderStatus>(request.NewStatus, true, out var newStatus))
            throw new InvalidTenderStatusException(request.NewStatus);

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
        await eventPublisher.PublishTenderStatusChangedAsync(tender.Id, tender.Title, tender.Status.ToString(), cancellationToken);

        return new ChangeTenderStatusCommandResponse(tender.Id, tender.Status.ToString(), tender.UpdatedAt);
    }
}
