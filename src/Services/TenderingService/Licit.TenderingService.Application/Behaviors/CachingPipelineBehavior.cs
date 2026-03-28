using System.Text.Json;
using FlashMediator;
using Licit.TenderingService.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Licit.TenderingService.Application.Behaviors;

public class CachingPipelineBehavior<TRequest, TResponse>(
    IDistributedCache cache,
    ILogger<CachingPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(5);

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not ICacheableQuery)
            return await next();

        var key = GenerateCacheKey(request);

        var cached = await cache.GetStringAsync(key, cancellationToken);
        if (cached is not null)
        {
            logger.LogDebug("Cache hit: {Key}", key);
            return JsonSerializer.Deserialize<TResponse>(cached)!;
        }

        logger.LogDebug("Cache miss: {Key}", key);
        var response = await next();

        await cache.SetStringAsync(
            key,
            JsonSerializer.Serialize(response),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = DefaultExpiration },
            cancellationToken);

        return response;
    }

    private static string GenerateCacheKey(TRequest request)
    {
        var typeName = typeof(TRequest).Name;
        var json = JsonSerializer.Serialize(request);
        return $"{typeName}:{json}";
    }
}
