using FlashMediator;
using FluentValidation;
using Licit.TenderingService.Application.Features.CQRS.Tender.ChangeStatus.Exceptions;
using Licit.TenderingService.Application.Features.CQRS.Tender.GetById.Exceptions;
using Licit.TenderingService.Application.Interfaces;
using Licit.TenderingService.Domain.Entities;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.ChangeStatus;

public class ChangeTenderStatusCommandHandler(
    ITenderRepository tenderRepository,
    IValidator<ChangeTenderStatusCommandRequest> validator) : IRequestHandler<ChangeTenderStatusCommandRequest, ChangeTenderStatusCommandResponse>
{
    public async Task<ChangeTenderStatusCommandResponse> Handle(ChangeTenderStatusCommandRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var tender = await tenderRepository.GetByIdAsync(request.Id)
            ?? throw new TenderNotFoundException(request.Id);

        if (!Enum.TryParse<TenderStatus>(request.NewStatus, true, out var newStatus))
            throw new InvalidTenderStatusException(request.NewStatus);

        try
        {
            tender.ChangeStatus(newStatus);
        }
        catch (InvalidOperationException ex) when (ex.Message.StartsWith("STATUS_TRANSITION_INVALID"))
        {
            var parts = ex.Message.Split(':');
            throw new InvalidStatusTransitionException(parts[1], parts[2]);
        }

        await tenderRepository.UpdateAsync(tender);

        return new ChangeTenderStatusCommandResponse(tender.Id, tender.Status.ToString(), tender.UpdatedAt);
    }
}
