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

        logger.LogInformation("CQRS request started. RequestType: {RequestType}", requestType);

        try
        {
            var response = await next();
            stopwatch.Stop();

            logger.LogInformation(
                "CQRS request succeeded. RequestType: {RequestType} ElapsedMs: {ElapsedMs}",
                requestType,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception exception)
        {
            stopwatch.Stop();

            logger.LogWarning(
                exception,
                "CQRS request failed. RequestType: {RequestType} ElapsedMs: {ElapsedMs} ExceptionType: {ExceptionType}",
                requestType,
                stopwatch.ElapsedMilliseconds,
                exception.GetType().FullName);

            throw;
        }
    }
}
