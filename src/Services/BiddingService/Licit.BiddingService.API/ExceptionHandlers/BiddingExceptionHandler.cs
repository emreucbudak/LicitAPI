using Licit.BiddingService.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Licit.BiddingService.API.ExceptionHandlers
{
    public class BiddingExceptionHandler(ILogger<BiddingExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not BidPlacementException bidPlacementException)
                return false;

            var statusCode = GetStatusCode(bidPlacementException.ErrorCode);
            var traceId = httpContext.TraceIdentifier;
            var path = httpContext.Request.Path.Value;

            logger.LogWarning(
                exception,
                "Bidding hatasi islendi. TraceId: {TraceId} Path: {Path} StatusCode: {StatusCode} ErrorCode: {ErrorCode} Message: {Message}",
                traceId,
                path,
                statusCode,
                bidPlacementException.ErrorCode,
                exception.Message);

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(new
            {
                statusCode,
                message = exception.Message,
                errorCode = bidPlacementException.ErrorCode,
                traceId
            }, cancellationToken);

            return true;
        }

        private static int GetStatusCode(string errorCode) =>
            errorCode switch
            {
                "BID_STATE_NOT_FOUND" => StatusCodes.Status404NotFound,
                "WALLET_NOT_FOUND" => StatusCodes.Status404NotFound,
                "AUCTION_NOT_ACTIVE" => StatusCodes.Status409Conflict,
                "BID_TOO_LOW" => StatusCodes.Status409Conflict,
                "INSUFFICIENT_BALANCE" => StatusCodes.Status422UnprocessableEntity,
                "WALLET_GRPC_UNAUTHORIZED" => StatusCodes.Status502BadGateway,
                _ => StatusCodes.Status422UnprocessableEntity
            };
    }
}
