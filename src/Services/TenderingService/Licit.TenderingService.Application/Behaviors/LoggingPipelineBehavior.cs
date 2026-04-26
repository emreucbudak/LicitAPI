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

        logger.LogInformation("CQRS request started: {RequestName}", requestName);

        try
        {
            var response = await next();

            stopwatch.Stop();
            logger.LogInformation(
                "CQRS request completed: {RequestName}. ElapsedMs: {ElapsedMs}",
                requestName,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            logger.LogError(
                exception,
                "CQRS request failed: {RequestName}. ElapsedMs: {ElapsedMs}. ExceptionType: {ExceptionType}",
                requestName,
                stopwatch.ElapsedMilliseconds,
                exception.GetType().Name);

            throw;
        }
    }
}
