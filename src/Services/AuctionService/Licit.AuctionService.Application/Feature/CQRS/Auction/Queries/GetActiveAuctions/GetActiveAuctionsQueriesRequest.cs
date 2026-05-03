using FlashMediator;

namespace Licit.AuctionService.Application.Feature.CQRS.Auction.Queries.GetActiveAuctions
{
    public class GetActiveAuctionsQueriesRequest : IRequest<IEnumerable<GetActiveAuctionsQueriesResponse>>
    {
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 10;

    }
}
