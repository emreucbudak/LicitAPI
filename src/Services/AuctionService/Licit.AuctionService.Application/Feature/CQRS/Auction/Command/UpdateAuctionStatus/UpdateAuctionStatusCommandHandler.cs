using FlashMediator;
using Licit.AuctionService.Application.Interface;
using Licit.AuctionService.Application.Repository;

namespace Licit.AuctionService.Application.Feature.CQRS.Auction.Command.UpdateAuctionStatus
{
    public class UpdateAuctionStatusCommandHandler(IAuctionRepository repo,IUnitOfWork unit) : IRequestHandler<UpdateAuctionStatusCommandRequest>
    {
        public async Task Handle(UpdateAuctionStatusCommandRequest request, CancellationToken cancellationToken)
        {
            var auction = await repo.GetAuctionByIdAsync(request.AuctionId);
            if (auction is null)
            {
              throw new Exception("Auction not found");
            }
            auction.Status = (Domain.Entities.AuctionStatus)request.Status;
            await repo.UpdateAuctionAsync(auction);
            await unit.SaveChangesAsync();
        }
    }
}
