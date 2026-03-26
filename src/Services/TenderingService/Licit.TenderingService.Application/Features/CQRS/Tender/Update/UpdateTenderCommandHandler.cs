using FlashMediator;
using Licit.TenderingService.Application.Interfaces;
using Licit.TenderingService.Domain.Entities;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.Update;

public class UpdateTenderCommandHandler(
    ITenderRepository tenderRepository) : IRequestHandler<UpdateTenderCommandRequest, UpdateTenderCommandResponse>
{
    public async Task<UpdateTenderCommandResponse> Handle(UpdateTenderCommandRequest request, CancellationToken cancellationToken)
    {
        var tender = await tenderRepository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException("İhale bulunamadı.");

        if (tender.Status != TenderStatus.Draft)
            throw new InvalidOperationException("Sadece taslak durumundaki ihaleler güncellenebilir.");

        if (request.EndDate <= request.StartDate)
            throw new InvalidOperationException("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");

        tender.Title = request.Title;
        tender.Description = request.Description;
        tender.StartingPrice = request.StartingPrice;
        tender.StartDate = request.StartDate;
        tender.EndDate = request.EndDate;
        tender.UpdatedAt = DateTime.UtcNow;

        if (request.Rules is not null)
        {
            tender.Rules.Clear();
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

        await tenderRepository.UpdateAsync(tender);

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
