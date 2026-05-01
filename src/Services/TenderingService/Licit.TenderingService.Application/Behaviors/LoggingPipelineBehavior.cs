using System.Diagnostics;
using FlashMediator;
using Microsoft.Extensions.Logging;

namespace Licit.TenderingService.Application.Behaviors;

public class LoggingPipelineBehavior<TRequest, TResponse>(
    ILogger<LoggingPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation("CQRS isteği başladı. İstek adı: {RequestName}", requestName);

        try
        {
            var response = await next();

            stopwatch.Stop();
            logger.LogInformation(
                "CQRS isteği tamamlandı. İstek adı: {RequestName}. SüreMs: {ElapsedMs}",
                requestName,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            logger.LogError(
                exception,
                "CQRS isteği başarısız oldu. İstek adı: {RequestName}. SüreMs: {ElapsedMs}. Hata tipi: {ExceptionType}",
                requestName,
                stopwatch.ElapsedMilliseconds,
                exception.GetType().Name);

            throw;
        }
    }
}
