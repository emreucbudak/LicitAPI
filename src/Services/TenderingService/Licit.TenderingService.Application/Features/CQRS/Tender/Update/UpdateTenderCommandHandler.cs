using FlashMediator;
using FluentValidation;
using Licit.TenderingService.Application.Features.CQRS.Tender.GetById.Exceptions;
using Licit.TenderingService.Application.Features.CQRS.Tender.Update.Exceptions;
using Licit.TenderingService.Application.Interfaces;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.Update;

public class UpdateTenderCommandHandler(
    IUnitOfWork unitOfWork,
    IValidator<UpdateTenderCommandRequest> validator) : IRequestHandler<UpdateTenderCommandRequest, UpdateTenderCommandResponse>
{
    public async Task<UpdateTenderCommandResponse> Handle(UpdateTenderCommandRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var tender = await unitOfWork.Tenders.GetByIdAsync(request.Id)
            ?? throw new TenderNotFoundException(request.Id);

        try
        {
            tender.UpdateDetails(request.Title, request.Description, request.StartingPrice, request.StartDate, request.EndDate);
        }
        catch (InvalidOperationException ex) when (ex.Message == "TENDER_NOT_EDITABLE")
        {
            throw new TenderNotEditableException();
        }

        if (request.Rules is not null)
        {
            tender.ClearRules();
            foreach (var rule in request.Rules)
                tender.AddRule(rule.Title, rule.Description, rule.IsRequired);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateTenderCommandResponse(
            tender.Id,
            tender.Title,
            tender.Description,
            tender.StartingPrice,
            tender.StartDate,
            tender.EndDate,
            tender.Status.ToString(),
            tender.UpdatedAt
        );
    }
}
