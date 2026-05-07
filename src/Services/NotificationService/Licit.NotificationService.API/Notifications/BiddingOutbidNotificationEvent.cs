namespace Licit.NotificationService.API.Notifications;

public sealed record BiddingOutbidNotificationEvent(
    string EventType,
    Guid AuctionId,
    Guid BidId,
    Guid NewBidderUserId,
    int Amount,
    DateTime PlacedAt,
    IReadOnlyList<Guid> RecipientUserIds,
    DateTime OccurredAt);
