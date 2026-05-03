using FlashMediator;

namespace Licit.AuctionService.Application.Feature.CQRS.Auction.Queries.GetActiveAuctions
{
    public class GetActiveAuctionsQueriesRequest : IRequest<IEnumerable<GetActiveAuctionsQueriesResponse>>
    {
    }
}
