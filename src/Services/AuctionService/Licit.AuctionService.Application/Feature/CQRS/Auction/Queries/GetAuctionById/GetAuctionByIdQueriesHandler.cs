using FlashMediator;
using Licit.AuctionService.Application.Repository;

namespace Licit.AuctionService.Application.Feature.CQRS.Auction.Queries.GetAuctionById
{
    public class GetAuctionByIdQueriesHandler(IAuctionRepository repo) : IRequestHandler<GetAuctionByIdQueriesRequest, GetAuctionByIdQueriesResponse>
    {
        public async Task<GetAuctionByIdQueriesResponse> Handle(GetAuctionByIdQueriesRequest request, CancellationToken cancellationToken)
        {
            Domain.Entities.Auction auction = await repo.GetAuctionByIdAsync(request.AuctionId);
            if (auction is null)
            {
                throw new Exception("Auction not found");

            }
            return new GetAuctionByIdQueriesResponse()
            {
                AuctionName = auction.AuctionName,
                StartPrice = auction.StartPrice,
                IncreaseAmount = auction.IncreaseAmount,
                StartDate = auction.StartDate,
                EndDate = auction.EndDate,
                Rules = auction.Rules,
                Description = auction.Description,
                WinnerBidId = auction.WinnerBidId,
                Status = auction.Status,
                CreatedByUserId = auction.CreatedByUserId,
                ImgUrls = auction.ImgUrls
            };

        }
    }
}
