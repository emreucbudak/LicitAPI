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

        logger.LogInformation("CQRS isteği başladı. İstek tipi: {RequestType}", requestName);

        try
        {
            var response = await next();
            stopwatch.Stop();

            logger.LogInformation(
                "CQRS isteği başarıyla tamamlandı. İstek tipi: {RequestType}, SüreMs: {ElapsedMs}",
                requestName,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception exception)
        {
            stopwatch.Stop();

            logger.LogError(
                exception,
                "CQRS isteği başarısız oldu. İstek tipi: {RequestType}, SüreMs: {ElapsedMs}, Mesaj: {Message}",
                requestName,
                stopwatch.ElapsedMilliseconds,
                exception.Message);

            throw;
        }
    }
}
