using Microsoft.AspNetCore.Diagnostics;

namespace Licit.AuthService.API.ExceptionHandlers;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        const int statusCode = StatusCodes.Status500InternalServerError;
        const string message = "Beklenmeyen bir hata olustu.";

        var traceId = httpContext.TraceIdentifier;
        var path = httpContext.Request.Path.Value ?? string.Empty;

        logger.LogError(
            exception,
            "Yakalanmamış hata işlendi. İz kimliği: {TraceId} Yol: {Path} Hata tipi: {ExceptionType} Mesaj: {Message}",
            traceId,
            path,
            exception.GetType().FullName,
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
