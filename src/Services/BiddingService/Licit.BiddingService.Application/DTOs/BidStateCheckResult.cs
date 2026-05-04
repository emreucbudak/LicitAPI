namespace Licit.BiddingService.Application.DTOs
{
    public record BidStateCheckResult(
        bool Success,
        string? ErrorCode,
        int CurrentHighestBid,
        int MinimumRequiredBid,
        int Version)
    {
        public static BidStateCheckResult Accepted(
            int currentHighestBid,
            int minimumRequiredBid,
            int version)
            => new(true, null, currentHighestBid, minimumRequiredBid, version);

        public static BidStateCheckResult Rejected(
            string errorCode,
            int currentHighestBid,
            int minimumRequiredBid,
            int version)
            => new(false, errorCode, currentHighestBid, minimumRequiredBid, version);
    }
}
