using FlashMediator;
using Licit.TenderingService.Application.Interfaces;
using Licit.TenderingService.Domain.Entities;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.Create;

public class CreateTenderCommandHandler(
    ITenderRepository tenderRepository) : IRequestHandler<CreateTenderCommandRequest, CreateTenderCommandResponse>
{
    public async Task<CreateTenderCommandResponse> Handle(CreateTenderCommandRequest request, CancellationToken cancellationToken)
    {
        if (request.EndDate <= request.StartDate)
            throw new InvalidOperationException("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");

        if (request.StartingPrice < 0)
            throw new InvalidOperationException("Başlangıç fiyatı negatif olamaz.");

        var tender = new Domain.Entities.Tender
        {
            Title = request.Title,
            Description = request.Description,
            StartingPrice = request.StartingPrice,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            CreatedByUserId = request.CreatedByUserId,
            Status = TenderStatus.Draft
        };

        if (request.Rules is { Count: > 0 })
        {
            foreach (var rule in request.Rules)
            {
                tender.Rules.Add(new TenderRule
                {
                    TenderId = tender.Id,
                    Title = rule.Title,
                    Description = rule.Description,
                    IsRequired = rule.IsRequired
                });
            }
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
