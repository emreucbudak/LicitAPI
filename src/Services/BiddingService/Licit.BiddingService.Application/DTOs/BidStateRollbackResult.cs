namespace Licit.BiddingService.Application.DTOs
{
    public record BidStateRollbackResult(bool Success, string? ErrorCode)
    {
        public static BidStateRollbackResult RolledBack()
            => new(true, null);

        public static BidStateRollbackResult Failed(string errorCode)
            => new(false, errorCode);
    }
}
