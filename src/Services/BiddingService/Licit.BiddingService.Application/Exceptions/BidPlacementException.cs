namespace Licit.BiddingService.Application.Exceptions
{
    public class BidPlacementException(string errorCode)
        : Exception($"Bid placement failed: {errorCode}")
    {
        public string ErrorCode { get; } = errorCode;
    }
}
