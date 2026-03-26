using FlashMediator;
using Licit.TenderingService.Application.Interfaces;
using Licit.TenderingService.Domain.Entities;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.Delete;

public class DeleteTenderCommandHandler(
    ITenderRepository tenderRepository) : IRequestHandler<DeleteTenderCommandRequest>
{
    public async Task Handle(DeleteTenderCommandRequest request, CancellationToken cancellationToken)
    {
        var tender = await tenderRepository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException("İhale bulunamadı.");

        if (tender.Status == TenderStatus.Active)
            throw new InvalidOperationException("Aktif bir ihale silinemez. Önce iptal edilmelidir.");

        await tenderRepository.DeleteAsync(request.Id);
    }
}
