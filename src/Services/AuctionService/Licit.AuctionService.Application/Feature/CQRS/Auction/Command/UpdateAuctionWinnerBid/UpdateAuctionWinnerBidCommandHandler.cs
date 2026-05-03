using FlashMediator;
using Licit.AuctionService.Application.Interface;
using Licit.AuctionService.Application.Repository;

namespace Licit.AuctionService.Application.Feature.CQRS.Auction.Command.UpdateAuctionWinnerBid
{
    public class UpdateAuctionWinnerBidCommandHandler(IAuctionRepository repo,IUnitOfWork unit) : IRequestHandler<UpdateAuctionWinnerBidCommandRequest>
    {
        public async Task Handle(UpdateAuctionWinnerBidCommandRequest request, CancellationToken cancellationToken)
        {
            Domain.Entities.Auction auction = await repo.GetAuctionByIdAsync(request.AuctionId);
            auction.UpdateWinnerBid(request.WinnerBidId);
             repo.UpdateAuctionAsync(auction);
            await unit.SaveChangesAsync();

        }
    }
}
