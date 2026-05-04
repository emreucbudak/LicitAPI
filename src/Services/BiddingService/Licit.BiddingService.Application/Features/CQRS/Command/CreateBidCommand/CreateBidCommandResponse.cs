namespace Licit.BiddingService.Application.Features.CQRS.Command.CreateBidCommand
{
    public record CreateBidCommandResponse(
        Guid BidId,
        Guid AuctionId,
        Guid BidderUserId,
        int Amount,
        DateTime PlacedAt,
        int CurrentHighestBid,
        int BidStateVersion,
        Guid WalletHoldId);
}
