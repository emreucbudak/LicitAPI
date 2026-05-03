using FlashMediator;

namespace Licit.AuctionService.Application.Feature.CQRS.Auction.Queries.GetAuctionById
{
    public record GetAuctionByIdQueriesRequest : IRequest<GetAuctionByIdQueriesResponse>
    {
        public Guid AuctionId { get; init; }
    }
}
