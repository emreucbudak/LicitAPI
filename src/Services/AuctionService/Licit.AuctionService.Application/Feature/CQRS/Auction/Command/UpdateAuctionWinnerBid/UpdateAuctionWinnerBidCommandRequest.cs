using FlashMediator;

namespace Licit.AuctionService.Application.Feature.CQRS.Auction.Command.UpdateAuctionWinnerBid
{
    public record UpdateAuctionWinnerBidCommandRequest : IRequest
    {
        public Guid WinnerBidId { get; init; }
        public Guid AuctionId { get; init; }
    }
}
