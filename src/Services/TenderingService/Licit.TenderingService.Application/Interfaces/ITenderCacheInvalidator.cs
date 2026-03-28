namespace Licit.TenderingService.Application.Interfaces;

public interface ITenderCacheInvalidator
{
    Task InvalidateAsync(CancellationToken cancellationToken = default);
}
