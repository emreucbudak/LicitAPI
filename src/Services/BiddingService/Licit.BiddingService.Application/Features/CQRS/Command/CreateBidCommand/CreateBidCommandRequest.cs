using FlashMediator;

namespace Licit.BiddingService.Application.Features.CQRS.Command.CreateBidCommand
{
    public class CreateBidCommandRequest : IRequest<CreateBidCommandResponse>
    {
        public Guid AuctionId { get; set; }
        public Guid BidderUserId { get; set; }
        public int Amount { get; set; }
        public string IdempotencyKey { get; set; } = string.Empty;
    }
}
