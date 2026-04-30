using Microsoft.AspNetCore.Diagnostics;

namespace Licit.WalletService.API.ExceptionHandlers;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        const int statusCode = StatusCodes.Status500InternalServerError;
        var traceId = httpContext.TraceIdentifier;
        var path = httpContext.Request.Path.Value;

        logger.LogError(
            exception,
            "Beklenmeyen hata yakalandı. TraceId: {TraceId} Path: {Path} ErrorType: {ErrorType} Message: {Message}",
            traceId,
            path,
            exception.GetType().FullName,
            exception.Message);

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new
        {
            statusCode,
            message = "Beklenmeyen bir hata olustu.",
            traceId
        }, cancellationToken);

        return true;
    }
}
