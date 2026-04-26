using Microsoft.AspNetCore.Diagnostics;

namespace Licit.MailService.API.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        const int statusCode = StatusCodes.Status500InternalServerError;
        const string message = "Unexpected server error.";

        var traceId = httpContext.TraceIdentifier;
        var path = httpContext.Request.Path.Value ?? string.Empty;

        logger.LogError(
            exception,
            "Unhandled exception. TraceId: {TraceId}, Path: {Path}, Message: {Message}",
            traceId,
            path,
            exception.Message);

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
