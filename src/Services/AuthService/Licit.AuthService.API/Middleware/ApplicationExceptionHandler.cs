using Licit.AuthService.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Licit.AuthService.API.Middleware;

public class ApplicationExceptionHandler(ILogger<ApplicationExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not BaseException baseException)
            return false;

        var traceId = httpContext.TraceIdentifier;
        var path = httpContext.Request.Path.Value ?? string.Empty;
        var logLevel = baseException.StatusCode >= StatusCodes.Status500InternalServerError
            ? LogLevel.Error
            : LogLevel.Warning;

        logger.Log(
            logLevel,
            exception,
            "Application exception handled. TraceId: {TraceId} Path: {Path} StatusCode: {StatusCode} ExceptionType: {ExceptionType} Message: {Message}",
            traceId,
            path,
            baseException.StatusCode,
            exception.GetType().FullName,
            baseException.Message);

        httpContext.Response.StatusCode = baseException.StatusCode;
        await httpContext.Response.WriteAsJsonAsync(new
        {
            statusCode = baseException.StatusCode,
            message = baseException.Message,
            traceId
        }, cancellationToken);

        return true;
    }
}
