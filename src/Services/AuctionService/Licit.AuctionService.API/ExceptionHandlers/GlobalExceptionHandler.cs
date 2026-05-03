using Microsoft.AspNetCore.Diagnostics;

namespace Licit.AuctionService.API.ExceptionHandlers
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async  ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            const int statusCode = StatusCodes.Status500InternalServerError;
            var traceId = httpContext.TraceIdentifier;

            logger.LogError(
                exception,
                "Beklenmeyen hata yakalandı. İz kimliği: {TraceId} Hata tipi: {ErrorType} Mesaj: {Message}",
                traceId,
                exception.GetType().FullName,
                exception.Message);

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(new
            {
                statusCode,
                message = "Beklenmeyen bir hata olustu.",
                traceId
            }, cancellationToken);

            return true;

        }
    }
}
