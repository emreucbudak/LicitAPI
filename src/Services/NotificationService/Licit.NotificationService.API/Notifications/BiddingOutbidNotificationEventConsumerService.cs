using DotNetCore.CAP;

namespace Licit.NotificationService.API.Notifications;

public sealed class BiddingOutbidNotificationEventConsumerService(
    INotificationService notificationService,
    ILogger<BiddingOutbidNotificationEventConsumerService> logger) : ICapSubscribe
{
    private const string RoutingKey = "bidding.bid.outbid-email.requested";
    public const string QueueName = "notification-service.bidding-outbid";

    [CapSubscribe(RoutingKey, Group = QueueName)]
    public async Task HandleAsync(
        BiddingOutbidNotificationEvent eventData,
        CancellationToken cancellationToken)
    {
        if (eventData.RecipientUserIds is null || eventData.RecipientUserIds.Count == 0)
        {
            return;
        }

        var recipientUserIds = eventData.RecipientUserIds.Distinct().ToArray();

        foreach (var recipientUserId in recipientUserIds)
        {
            await notificationService.PublishAsync(
                new CreateNotificationRequest(
                    recipientUserId.ToString(),
                    "auction.outbid",
                    "Teklifiniz gecildi",
                    $"Takip ettiginiz ihalede {eventData.Amount:N0} TL tutarinda yeni bir teklif verildi.",
                    $"/auctions/{eventData.AuctionId}",
                    new Dictionary<string, string>
                    {
                        ["auctionId"] = eventData.AuctionId.ToString(),
                        ["bidId"] = eventData.BidId.ToString(),
                        ["newBidderUserId"] = eventData.NewBidderUserId.ToString(),
                        ["amount"] = eventData.Amount.ToString(),
                        ["placedAt"] = eventData.PlacedAt.ToString("O"),
                        ["eventType"] = eventData.EventType,
                        ["occurredAt"] = eventData.OccurredAt.ToString("O")
                    }),
                cancellationToken);
        }

        logger.LogInformation(
            "Bidding outbid site notifications processed. AuctionId: {AuctionId}, RecipientCount: {RecipientCount}",
            eventData.AuctionId,
            recipientUserIds.Length);
    }
}
