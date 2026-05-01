using FlashMediator;
using FluentValidation;
using Licit.TenderingService.Application.Exceptions;
using Licit.TenderingService.Application.Interfaces;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.Commands.Create;

public class CreateTenderCommandHandler(
    IUnitOfWork unitOfWork,
    ITenderRepository tenderRepository,
    IValidator<CreateTenderCommandRequest> validator,
    ICurrentUserService currentUserService,
    ITenderCacheInvalidator cacheInvalidator) : IRequestHandler<CreateTenderCommandRequest, CreateTenderCommandResponse>
{
    public async Task<CreateTenderCommandResponse> Handle(CreateTenderCommandRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("Kullanici kimligi bulunamadi.");

        var tender = new Domain.Entities.Tender(
            request.Title,
            request.Description,
            request.StartingPrice,
            request.StartDate,
            request.EndDate,
            userId,
            request.CategoryId
        );

        if (request.Rules is { Count: > 0 })
        {
            foreach (var rule in request.Rules)
                tender.AddRule(rule.Title, rule.Description, rule.IsRequired);
        }

        tenderRepository.Add(tender);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cacheInvalidator.InvalidateAsync(cancellationToken);

        return new CreateTenderCommandResponse(
            tender.Id,
            tender.Title,
            tender.Description,
            tender.StartingPrice,
            tender.StartDate,
            tender.EndDate,
            tender.Status.ToString(),
            tender.CategoryId,
            tender.ImageUrl,
            tender.CreatedAt
        );
    }
}
