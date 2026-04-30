using Licit.WalletService.Application.Exceptions;
using Licit.WalletService.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Licit.WalletService.API.ExceptionHandlers;

public class CustomExceptionHandler(ILogger<CustomExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (handled, statusCode, message) = exception switch
        {
            BaseException applicationException => (true, applicationException.StatusCode, applicationException.Message),
            InvalidAmountException or InsufficientBalanceException or InsufficientFrozenBalanceException
                => (true, StatusCodes.Status422UnprocessableEntity, exception.Message),
            _ => (false, 0, string.Empty)
        };

        if (!handled)
            return false;

        var traceId = httpContext.TraceIdentifier;
        var path = httpContext.Request.Path.Value;
        var errorType = exception.GetType().FullName;

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Uygulama/domain hatası işlendi. TraceId: {TraceId} Path: {Path} StatusCode: {StatusCode} ErrorType: {ErrorType} Message: {Message}",
                traceId,
                path,
                statusCode,
                errorType,
                message);
        }
        else
        {
            logger.LogWarning(
                exception,
                "Uygulama/domain hatası işlendi. TraceId: {TraceId} Path: {Path} StatusCode: {StatusCode} ErrorType: {ErrorType} Message: {Message}",
                traceId,
                path,
                statusCode,
                errorType,
                message);
        }

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
