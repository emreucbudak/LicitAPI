namespace Licit.BiddingService.Domain.Entities
{
    public class Bid
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();
        public Guid AuctionId { get; set; }
        public Guid BidderUserId { get; set; }
        public int Amount { get; set; }
        public DateTime PlacedAt { get; set; } = DateTime.UtcNow;
        public string IdempotencyKey { get; set; } = null!;
        public Guid? WalletHoldId { get; set; }
    }
}
