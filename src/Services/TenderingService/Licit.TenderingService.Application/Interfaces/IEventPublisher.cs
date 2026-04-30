namespace Licit.TenderingService.Application.Interfaces;

public interface IEventPublisher
{
    Task PublishTenderStatusChangedAsync(Guid tenderId, string title, string newStatus, string? imageUrl = null, CancellationToken cancellationToken = default);
}
