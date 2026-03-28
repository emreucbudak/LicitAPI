using Licit.TenderingService.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace Licit.TenderingService.Infrastructure.Services;

public class TenderCacheInvalidator(IDistributedCache cache) : ITenderCacheInvalidator
{
    public async Task InvalidateAsync(CancellationToken cancellationToken = default)
    {
        // Invalidate common pagination cache keys
        for (var page = 1; page <= 10; page++)
        {
            foreach (var pageSize in new[] { 10, 20, 50 })
            {
                await cache.RemoveAsync($"tenders:all:{page}:{pageSize}", cancellationToken);
            }
        }
    }
}
