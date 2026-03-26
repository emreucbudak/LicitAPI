using FlashMediator;
using FluentValidation;
using Licit.TenderingService.Application.Interfaces;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.Create;

public class CreateTenderCommandHandler(
    ITenderRepository tenderRepository,
    IValidator<CreateTenderCommandRequest> validator) : IRequestHandler<CreateTenderCommandRequest, CreateTenderCommandResponse>
{
    public async Task<CreateTenderCommandResponse> Handle(CreateTenderCommandRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var tender = new Domain.Entities.Tender(
            request.Title,
            request.Description,
            request.StartingPrice,
            request.StartDate,
            request.EndDate,
            request.CreatedByUserId
        );

        if (request.Rules is { Count: > 0 })
        {
            foreach (var rule in request.Rules)
                tender.AddRule(rule.Title, rule.Description, rule.IsRequired);
        }

        var created = await tenderRepository.CreateAsync(tender);

        return new CreateTenderCommandResponse(
            created.Id,
            created.Title,
            created.Description,
            created.StartingPrice,
            created.StartDate,
            created.EndDate,
            created.Status.ToString(),
            created.CreatedAt
        );
    }
}
