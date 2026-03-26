using FlashMediator;
using FluentValidation;
using Licit.TenderingService.Application.Features.CQRS.Tender.GetById.Exceptions;
using Licit.TenderingService.Application.Interfaces;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.GetById;

public class GetTenderByIdQueryHandler(
    IUnitOfWork unitOfWork,
    IValidator<GetTenderByIdQueryRequest> validator) : IRequestHandler<GetTenderByIdQueryRequest, GetTenderByIdQueryResponse>
{
    public async Task<GetTenderByIdQueryResponse> Handle(GetTenderByIdQueryRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var tender = await unitOfWork.Tenders.GetByIdAsync(request.Id)
            ?? throw new TenderNotFoundException(request.Id);

        return new GetTenderByIdQueryResponse(
            tender.Id,
            tender.Title,
            tender.Description,
            tender.StartingPrice,
            tender.StartDate,
            tender.EndDate,
            tender.Status.ToString(),
            tender.CreatedByUserId,
            tender.CreatedAt,
            tender.UpdatedAt,
            tender.Rules.Select(r => new TenderRuleDto(r.Id, r.Title, r.Description, r.IsRequired)).ToList()
        );
    }
}
