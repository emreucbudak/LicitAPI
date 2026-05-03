namespace Licit.BiddingService.Application.Notifier
{
    public interface IBiddingNotifier
    {
        Task NotifyBidPlacedAsync(
            Guid auctionId,
            Guid bidId,
            Guid bidderUserId,
            int amount,
            DateTime placedAt,
            CancellationToken cancellationToken = default);
    }
}
