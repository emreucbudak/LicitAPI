using FlashMediator;
using FluentValidation;
using Licit.TenderingService.Application.Exceptions;
using Licit.TenderingService.Application.Features.CQRS.Tender.GetById.Exceptions;
using Licit.TenderingService.Application.Interfaces;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.Delete;

public class DeleteTenderCommandHandler(
    IUnitOfWork unitOfWork,
    IValidator<DeleteTenderCommandRequest> validator,
    ITenderCacheInvalidator cacheInvalidator) : IRequestHandler<DeleteTenderCommandRequest>
{
    public async Task Handle(DeleteTenderCommandRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var tender = await unitOfWork.Tenders.GetByIdAsync(request.Id)
            ?? throw new TenderNotFoundException(request.Id);

        if (tender.CreatedByUserId != request.UserId)
            throw new ForbiddenException("Bu ihaleyi yalnızca sahibi silebilir.");

        tender.ValidateForDeletion();

        unitOfWork.Tenders.Remove(tender);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cacheInvalidator.InvalidateAsync(cancellationToken);
    }
}
