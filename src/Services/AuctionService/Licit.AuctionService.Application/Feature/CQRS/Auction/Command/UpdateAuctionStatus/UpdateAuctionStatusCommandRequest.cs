using FlashMediator;

namespace Licit.AuctionService.Application.Feature.CQRS.Auction.Command.UpdateAuctionStatus
{
    public class UpdateAuctionStatusCommandRequest : IRequest
    {
        public Guid AuctionId { get; set; }
        public int Status { get; set; }
    }
}
