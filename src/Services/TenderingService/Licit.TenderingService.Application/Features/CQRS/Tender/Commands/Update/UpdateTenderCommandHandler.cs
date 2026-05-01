using FlashMediator;
using FluentValidation;
using Licit.TenderingService.Application.Exceptions;
using Licit.TenderingService.Application.Features.CQRS.Tender.Queries.GetById.Exceptions;
using Licit.TenderingService.Application.Interfaces;
using Licit.TenderingService.Domain.Exceptions;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.Commands.Update;

public class UpdateTenderCommandHandler(
    IUnitOfWork unitOfWork,
    ITenderRepository tenderRepository,
    IValidator<UpdateTenderCommandRequest> validator,
    ICurrentUserService currentUserService,
    ITenderCacheInvalidator cacheInvalidator) : IRequestHandler<UpdateTenderCommandRequest, UpdateTenderCommandResponse>
{
    public async Task<UpdateTenderCommandResponse> Handle(UpdateTenderCommandRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var tender = await tenderRepository.GetByIdAsync(request.Id)
            ?? throw new TenderNotFoundException(request.Id);

        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("Kullanici kimligi bulunamadi.");

        if (tender.CreatedByUserId != userId)
            throw new ForbiddenException("Bu ihaleyi yalnızca sahibi güncelleyebilir.");

        tender.UpdateDetails(request.Title, request.Description, request.StartingPrice, request.StartDate, request.EndDate, request.CategoryId);

        if (request.Rules is not null)
        {
            tender.ClearRules();
            foreach (var rule in request.Rules)
                tender.AddRule(rule.Title, rule.Description, rule.IsRequired);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cacheInvalidator.InvalidateAsync(cancellationToken);

        return new UpdateTenderCommandResponse(
            tender.Id,
            tender.Title,
            tender.Description,
            tender.StartingPrice,
            tender.StartDate,
            tender.EndDate,
            tender.Status.ToString(),
            tender.CategoryId,
            tender.ImageUrl,
            tender.UpdatedAt
        );
    }
}
