using FlashMediator;
using Licit.AuctionService.Application.Repository;

namespace Licit.AuctionService.Application.Feature.CQRS.Auction.Queries.GetActiveAuctions
{
    public class GetActiveAuctionsQueriesHandler(IAuctionRepository repository) : IRequestHandler<GetActiveAuctionsQueriesRequest, IEnumerable<GetActiveAuctionsQueriesResponse>>
    {
        public async Task<IEnumerable<GetActiveAuctionsQueriesResponse>> Handle(GetActiveAuctionsQueriesRequest request, CancellationToken cancellationToken)
        {
            var activeAuctions = await repository.GetActiveAuctions();
            var auctions = activeAuctions.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize);
            return auctions.Select(auction => new GetActiveAuctionsQueriesResponse
            {
                AuctionName = auction.AuctionName,
                StartPrice = auction.StartPrice,
                IncreaseAmount = auction.IncreaseAmount,
                StartDate = auction.StartDate,
                EndDate = auction.EndDate,
                Rules = auction.Rules,
                Description = auction.Description,
                CreatedByUserId = auction.CreatedByUserId,
                ImgUrls = auction.ImgUrls
            });
        }
    }
}
