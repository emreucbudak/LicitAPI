using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

namespace Licit.TenderingService.API.Middleware;

public class ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
            return false;

        const int statusCode = StatusCodes.Status400BadRequest;
        const string message = "Validation failed.";

        var traceId = httpContext.TraceIdentifier;
        var path = httpContext.Request.Path.Value;
        var failures = validationException.Errors.ToArray();
        var errors = failures
            .GroupBy(error => string.IsNullOrWhiteSpace(error.PropertyName) ? "Request" : error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(error => error.ErrorMessage)
                    .Distinct()
                    .ToArray());

        logger.LogWarning(
            validationException,
            "Validation exception on {Path}. Errors: {@ValidationErrors}. TraceId: {TraceId}",
            path,
            failures.Select(error => new
            {
                error.PropertyName,
                error.ErrorCode,
                error.ErrorMessage
            }).ToArray(),
            traceId);

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new
        {
            statusCode,
            message,
            traceId,
            errors
        }, cancellationToken);

        return true;
    }
}
