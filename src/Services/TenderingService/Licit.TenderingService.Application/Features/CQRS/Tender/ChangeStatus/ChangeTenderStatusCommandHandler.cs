using FlashMediator;
using Licit.TenderingService.Application.Interfaces;
using Licit.TenderingService.Domain.Entities;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.ChangeStatus;

public class ChangeTenderStatusCommandHandler(
    ITenderRepository tenderRepository) : IRequestHandler<ChangeTenderStatusCommandRequest, ChangeTenderStatusCommandResponse>
{
    public async Task<ChangeTenderStatusCommandResponse> Handle(ChangeTenderStatusCommandRequest request, CancellationToken cancellationToken)
    {
        var tender = await tenderRepository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException("İhale bulunamadı.");

        if (!Enum.TryParse<TenderStatus>(request.NewStatus, true, out var newStatus))
            throw new InvalidOperationException($"Geçersiz durum: {request.NewStatus}");

        ValidateStatusTransition(tender.Status, newStatus);

        tender.Status = newStatus;
        tender.UpdatedAt = DateTime.UtcNow;

        await tenderRepository.UpdateAsync(tender);

        return new ChangeTenderStatusCommandResponse(tender.Id, tender.Status.ToString(), tender.UpdatedAt);
    }

    private static void ValidateStatusTransition(TenderStatus current, TenderStatus next)
    {
        var allowed = current switch
        {
            TenderStatus.Draft => next is TenderStatus.Active or TenderStatus.Cancelled,
            TenderStatus.Active => next is TenderStatus.Closed or TenderStatus.Cancelled,
            TenderStatus.Closed => next is TenderStatus.Completed,
            _ => false
        };

        if (!allowed)
            throw new InvalidOperationException($"'{current}' durumundan '{next}' durumuna geçiş yapılamaz.");
    }
}
