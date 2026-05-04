namespace Licit.MailService.Application.DTOs;

public sealed record BiddingOutbidEmailEvent(
    string EventType,
    Guid AuctionId,
    Guid BidId,
    Guid NewBidderUserId,
    int Amount,
    DateTime PlacedAt,
    IReadOnlyList<Guid> RecipientUserIds,
    DateTime OccurredAt);
