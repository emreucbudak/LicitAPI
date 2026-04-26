using System.Diagnostics;
using FlashMediator;
using Microsoft.Extensions.Logging;

namespace Licit.MailService.Application.Behaviors;

public class LoggingPipelineBehavior<TRequest, TResponse>(ILogger<LoggingPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation("CQRS request started. RequestType: {RequestType}", requestName);

        try
        {
            var response = await next();
            stopwatch.Stop();

            logger.LogInformation(
                "CQRS request succeeded. RequestType: {RequestType}, ElapsedMs: {ElapsedMs}",
                requestName,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception exception)
        {
            stopwatch.Stop();

            logger.LogError(
                exception,
                "CQRS request failed. RequestType: {RequestType}, ElapsedMs: {ElapsedMs}, Message: {Message}",
                requestName,
                stopwatch.ElapsedMilliseconds,
                exception.Message);

            throw;
        }
    }
}
