namespace Licit.BiddingService.Domain.Entities
{
    public class Bid
    {
        public Bid()
        {
        }

        public Bid(Guid auctionId, Guid bidderUserId, int amount, string idempotencyKey)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
            ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);

            AuctionId = auctionId;
            BidderUserId = bidderUserId;
            Amount = amount;
            IdempotencyKey = idempotencyKey.Trim();
            PlacedAt = DateTime.UtcNow;
        }

        public Guid Id { get; set; } = Guid.CreateVersion7();
        public Guid AuctionId { get; set; }
        public Guid BidderUserId { get; set; }
        public int Amount { get; set; }
        public DateTime PlacedAt { get; set; } = DateTime.UtcNow;
        public string IdempotencyKey { get; set; } = null!;
        public Guid? WalletHoldId { get; set; }

        public void AttachWalletHold(Guid walletHoldId)
        {
            if (walletHoldId == Guid.Empty)
                throw new ArgumentException("Wallet hold id cannot be empty.", nameof(walletHoldId));

            WalletHoldId = walletHoldId;
        }
    }
}
