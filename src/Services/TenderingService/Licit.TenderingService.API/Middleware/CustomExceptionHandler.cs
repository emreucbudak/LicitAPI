using Licit.TenderingService.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Licit.TenderingService.API.Middleware;

public class CustomExceptionHandler(ILogger<CustomExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (!TryGetExceptionDetails(exception, out var statusCode, out var message))
            return false;

        var traceId = httpContext.TraceIdentifier;
        var path = httpContext.Request.Path.Value;

        logger.LogWarning(
            exception,
            "Handled custom exception {ExceptionType} on {Path}. StatusCode: {StatusCode}. Message: {Message}. TraceId: {TraceId}",
            exception.GetType().Name,
            path,
            statusCode,
            message,
            traceId);

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new
        {
            statusCode,
            message,
            traceId
        }, cancellationToken);

        return true;
    }

    private static bool TryGetExceptionDetails(Exception exception, out int statusCode, out string message)
    {
        if (exception is BaseException baseException)
        {
            statusCode = baseException.StatusCode;
            message = baseException.Message;
            return true;
        }

        if (IsTenderingDomainException(exception))
        {
            statusCode = StatusCodes.Status422UnprocessableEntity;
            message = exception.Message;
            return true;
        }

        statusCode = default;
        message = string.Empty;
        return false;
    }

    private static bool IsTenderingDomainException(Exception exception)
    {
        return string.Equals(
            exception.GetType().Namespace,
            "Licit.TenderingService.Domain.Exceptions",
            StringComparison.Ordinal);
    }
}
