using FlashMediator;
using Licit.TenderingService.Application.Interfaces;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.GetById;

public class GetTenderByIdQueryHandler(
    ITenderRepository tenderRepository) : IRequestHandler<GetTenderByIdQueryRequest, GetTenderByIdQueryResponse>
{
    public async Task<GetTenderByIdQueryResponse> Handle(GetTenderByIdQueryRequest request, CancellationToken cancellationToken)
    {
        var tender = await tenderRepository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException("İhale bulunamadı.");

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
