using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

namespace Licit.MailService.API.Middleware;

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
            .GroupBy(error => error.PropertyName)
            .Select(group => new
            {
                propertyName = group.Key,
                messages = group.Select(error => error.ErrorMessage).ToArray()
            })
            .ToArray();

        logger.LogWarning(
            validationException,
            "Validation exception. TraceId: {TraceId}, Path: {Path}, Errors: {@Errors}",
            traceId,
            path,
            errors);

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new
        {
            statusCode,
            message = "Validation failed.",
            errors,
            traceId
        }, cancellationToken);

        return true;
    }
}
