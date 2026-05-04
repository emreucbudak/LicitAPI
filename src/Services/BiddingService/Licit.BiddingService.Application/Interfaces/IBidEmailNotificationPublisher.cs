namespace Licit.BiddingService.Application.Interfaces
{
    public interface IBidEmailNotificationPublisher
    {
        Task PublishOutbidEmailRequestedAsync(
            Guid auctionId,
            Guid bidId,
            Guid newBidderUserId,
            int amount,
            DateTime placedAt,
            IReadOnlyCollection<Guid> recipientUserIds,
            CancellationToken cancellationToken = default);
    }
}
