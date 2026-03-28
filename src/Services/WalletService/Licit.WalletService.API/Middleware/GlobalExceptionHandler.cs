using FluentValidation;
using Licit.WalletService.Application.Exceptions;
using Licit.WalletService.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Licit.WalletService.API.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, message) = exception switch
        {
            ValidationException validationEx => (400, string.Join(" | ", validationEx.Errors.Select(e => e.ErrorMessage))),
            BaseException baseEx => (baseEx.StatusCode, baseEx.Message),
            InvalidAmountException or InsufficientBalanceException or InsufficientFrozenBalanceException
                => (422, exception.Message),
            _ => (500, "Beklenmeyen bir hata oluştu.")
        };

        if (statusCode >= 500)
            logger.LogError(exception, "Sunucu hatası oluştu. TraceId: {TraceId}", httpContext.TraceIdentifier);
        else if (exception is not ValidationException)
            logger.LogWarning("İş kuralı hatası: {Message} TraceId: {TraceId}", message, httpContext.TraceIdentifier);

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new
        {
            error = message,
            statusCode,
            traceId = httpContext.TraceIdentifier
        }, cancellationToken);

        return true;
    }
}
