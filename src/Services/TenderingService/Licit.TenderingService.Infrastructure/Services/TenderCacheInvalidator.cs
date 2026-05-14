using Licit.TenderingService.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Licit.TenderingService.Infrastructure.Services;

public class TenderCacheInvalidator(
    IDistributedCache cache,
    ILogger<TenderCacheInvalidator> logger) : ITenderCacheInvalidator
{
    public async Task InvalidateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            for (var page = 1; page <= 10; page++)
            {
                foreach (var pageSize in new[] { 10, 20, 50 })
                {
                    await cache.RemoveAsync($"tenders:all:{page}:{pageSize}", cancellationToken);
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Tender cache invalidation failed. Continuing with the saved tender change.");
        }
    }
}
