namespace Licit.BiddingService.Application.DTOs
{
    public record BidStateUpdateResult(
        bool Success,
        string? ErrorCode,
        int CurrentHighestBid,
        int Version)
    {
        public static BidStateUpdateResult Updated(int currentHighestBid, int version)
            => new(true, null, currentHighestBid, version);

        public static BidStateUpdateResult Rejected(
            string errorCode,
            int currentHighestBid,
            int version)
            => new(false, errorCode, currentHighestBid, version);
    }
}
