using FlashMediator;
using FluentValidation;
using Licit.TenderingService.Application.Features.CQRS.Tender.Queries.GetById.Exceptions;
using Licit.TenderingService.Application.Interfaces;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.Queries.GetById;

public class GetTenderByIdQueryHandler(
    ITenderRepository tenderRepository,
    IValidator<GetTenderByIdQueryRequest> validator) : IRequestHandler<GetTenderByIdQueryRequest, GetTenderByIdQueryResponse>
{
    public async Task<GetTenderByIdQueryResponse> Handle(GetTenderByIdQueryRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var tender = await tenderRepository.GetByIdAsync(request.Id)
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
            tender.CategoryId,
            tender.Category?.Name ?? string.Empty,
            tender.ImageUrl,
            tender.CreatedAt,
            tender.UpdatedAt
        );
    }
}
