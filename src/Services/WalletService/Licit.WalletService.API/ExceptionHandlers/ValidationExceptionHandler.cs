using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

namespace Licit.WalletService.API.ExceptionHandlers;

public class ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
            return false;

        const int statusCode = StatusCodes.Status400BadRequest;
        var traceId = httpContext.TraceIdentifier;
        var path = httpContext.Request.Path.Value;
        var errors = validationException.Errors
            .Select(error => new
            {
                propertyName = error.PropertyName,
                message = error.ErrorMessage,
                errorCode = error.ErrorCode
            })
            .ToArray();

        logger.LogWarning(
            validationException,
            "Doğrulama hatası işlendi. TraceId: {TraceId} Path: {Path} Errors: {@Errors}",
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
}
