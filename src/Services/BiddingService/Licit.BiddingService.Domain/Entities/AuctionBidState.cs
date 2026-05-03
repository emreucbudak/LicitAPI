namespace Licit.BiddingService.Domain.Entities
{
    public class AuctionBidState
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();
        public Guid AuctionId { get; set; }
        public Guid? HighestBidId { get; set; }
        public Guid? HighestBidderUserId { get; set; }
        public int HighestBidAmount { get; set; }
        public int Version { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool CanAcceptBid(int amount, int minimumIncreaseAmount)
        {
            return amount >= HighestBidAmount + minimumIncreaseAmount;
        }

        public void ApplyHighestBid(Guid bidId, Guid bidderUserId, int amount)
        {
            HighestBidId = bidId;
            HighestBidderUserId = bidderUserId;
            HighestBidAmount = amount;
            Version++;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
