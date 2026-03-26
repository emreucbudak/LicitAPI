using FluentValidation;
using Licit.MailService.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Licit.MailService.API.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, message) = exception switch
        {
            ValidationException validationEx => (400, string.Join(" | ", validationEx.Errors.Select(e => e.ErrorMessage))),
            BaseException baseEx => (baseEx.StatusCode, baseEx.Message),
            _ => (500, "Beklenmeyen bir hata oluştu.")
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new
        {
            error = message,
            statusCode
        }, cancellationToken);

        return true;
    }
}
