using System.Diagnostics;
using FlashMediator;
using Microsoft.Extensions.Logging;

namespace Licit.AuthService.Application.Behaviors;

public class LoggingPipelineBehavior<TRequest, TResponse>(
    ILogger<LoggingPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest).FullName ?? typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation("CQRS isteği başladı. İstek tipi: {RequestType}", requestType);

        try
        {
            var response = await next();
            stopwatch.Stop();

            logger.LogInformation(
                "CQRS isteği başarıyla tamamlandı. İstek tipi: {RequestType} SüreMs: {ElapsedMs}",
                requestType,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception exception)
        {
            stopwatch.Stop();

            logger.LogWarning(
                exception,
                "CQRS isteği başarısız oldu. İstek tipi: {RequestType} SüreMs: {ElapsedMs} Hata tipi: {ExceptionType}",
                requestType,
                stopwatch.ElapsedMilliseconds,
                exception.GetType().FullName);

            throw;
        }
    }
}
