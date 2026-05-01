using FlashMediator;
using FluentValidation;
using Licit.TenderingService.Application.Exceptions;
using Licit.TenderingService.Application.Features.CQRS.Tender.Queries.GetById.Exceptions;
using Licit.TenderingService.Application.Interfaces;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.Commands.Delete;

public class DeleteTenderCommandHandler(
    IUnitOfWork unitOfWork,
    ITenderRepository tenderRepository,
    IValidator<DeleteTenderCommandRequest> validator,
    ICurrentUserService currentUserService,
    ITenderCacheInvalidator cacheInvalidator) : IRequestHandler<DeleteTenderCommandRequest>
{
    public async Task Handle(DeleteTenderCommandRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var tender = await tenderRepository.GetByIdAsync(request.Id)
            ?? throw new TenderNotFoundException(request.Id);

        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("Kullanici kimligi bulunamadi.");

        if (tender.CreatedByUserId != userId)
            throw new ForbiddenException("Bu ihaleyi yalnızca sahibi silebilir.");

        tender.ValidateForDeletion();

        tenderRepository.Remove(tender);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cacheInvalidator.InvalidateAsync(cancellationToken);
    }
}
