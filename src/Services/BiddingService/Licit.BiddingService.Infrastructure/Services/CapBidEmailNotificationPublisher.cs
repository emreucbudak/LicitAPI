using DotNetCore.CAP;
using Licit.BiddingService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Licit.BiddingService.Infrastructure.Services
{
    public class CapBidEmailNotificationPublisher(
        ICapPublisher capPublisher,
        ILogger<CapBidEmailNotificationPublisher> logger) : IBidEmailNotificationPublisher
    {
        private const string OutbidEmailRequestedEventName = "bidding.bid.outbid-email.requested";

        public async Task PublishOutbidEmailRequestedAsync(
            Guid auctionId,
            Guid bidId,
            Guid newBidderUserId,
            int amount,
            DateTime placedAt,
            IReadOnlyCollection<Guid> recipientUserIds,
            CancellationToken cancellationToken = default)
        {
            if (recipientUserIds.Count == 0)
                return;

            var message = new BidOutbidEmailRequestedEvent(
                "BidOutbidEmailRequested",
                auctionId,
                bidId,
                newBidderUserId,
                amount,
                placedAt,
                recipientUserIds.ToArray(),
                DateTime.UtcNow);

            await capPublisher.PublishAsync(
                OutbidEmailRequestedEventName,
                message,
                callbackName: null,
                cancellationToken);

            logger.LogInformation(
                "Queued bid outbid email request in CAP outbox. AuctionId: {AuctionId}, BidId: {BidId}, RecipientCount: {RecipientCount}",
                auctionId,
                bidId,
                recipientUserIds.Count);
        }

        private sealed record BidOutbidEmailRequestedEvent(
            string EventType,
            Guid AuctionId,
            Guid BidId,
            Guid NewBidderUserId,
            int Amount,
            DateTime PlacedAt,
            IReadOnlyList<Guid> RecipientUserIds,
            DateTime OccurredAt);
    }
}
