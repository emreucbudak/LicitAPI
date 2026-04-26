using Microsoft.AspNetCore.Diagnostics;

namespace Licit.TenderingService.API.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        const int statusCode = StatusCodes.Status500InternalServerError;
        const string message = "Beklenmeyen bir hata olustu.";

        var traceId = httpContext.TraceIdentifier;
        var path = httpContext.Request.Path.Value;

        logger.LogError(
            exception,
            "Unhandled exception {ExceptionType} on {Path}. Message: {Message}. TraceId: {TraceId}",
            exception.GetType().Name,
            path,
            exception.Message,
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
}
