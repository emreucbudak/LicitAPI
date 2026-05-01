using Licit.MailService.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Licit.MailService.API.ExceptionHandlers;

public class ApplicationExceptionHandler(ILogger<ApplicationExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not BaseException baseException)
            return false;

        var traceId = httpContext.TraceIdentifier;
        var path = httpContext.Request.Path.Value ?? string.Empty;
        var statusCode = baseException.StatusCode;

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                baseException,
                "Uygulama hatası. İz kimliği: {TraceId}, Yol: {Path}, Durum kodu: {StatusCode}, Mesaj: {Message}",
                traceId,
                path,
                statusCode,
                baseException.Message);
        }
        else
        {
            logger.LogWarning(
                baseException,
                "Uygulama hatası. İz kimliği: {TraceId}, Yol: {Path}, Durum kodu: {StatusCode}, Mesaj: {Message}",
                traceId,
                path,
                statusCode,
                baseException.Message);
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new
        {
            statusCode,
            message = baseException.Message,
            traceId
        }, cancellationToken);

        return true;
    }
}
