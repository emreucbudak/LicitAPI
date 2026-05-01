using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

namespace Licit.AuthService.API.ExceptionHandlers;

public class ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
            return false;

        const int statusCode = StatusCodes.Status400BadRequest;
        var traceId = httpContext.TraceIdentifier;
        var path = httpContext.Request.Path.Value ?? string.Empty;
        var errors = validationException.Errors
            .Select(error => new ValidationError(
                error.PropertyName,
                error.ErrorMessage,
                error.ErrorCode))
            .ToArray();

        logger.LogWarning(
            exception,
            "Doğrulama hatası işlendi. İz kimliği: {TraceId} Yol: {Path} Hatalar: {@Errors}",
            traceId,
            path,
            errors);

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new
        {
            statusCode,
            message = "Validation failed.",
            traceId,
            errors
        }, cancellationToken);

        return true;
    }

    private sealed record ValidationError(string PropertyName, string ErrorMessage, string ErrorCode);
}
