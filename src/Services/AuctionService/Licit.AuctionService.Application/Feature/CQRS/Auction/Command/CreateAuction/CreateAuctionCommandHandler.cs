using FlashMediator;
using Licit.AuctionService.Application.Interface;
using Licit.AuctionService.Application.Repository;

namespace Licit.AuctionService.Application.Feature.CQRS.Auction.Command.CreateAuction
{
    public class CreateAuctionCommandHandler(IAuctionRepository repo,IUnitOfWork unit) : IRequestHandler<CreateAuctionCommandRequest>
    {
        public async  Task Handle(CreateAuctionCommandRequest request, CancellationToken cancellationToken)
        {
            var auction = new Domain.Entities.Auction(request.AuctionName, request.StartPrice, request.IncreaseAmount, request.StartDate, request.EndDate, request.Rules, request.Description, request.Status, request.CreatedByUserId, request.ImgUrls);
            await repo.CreateAuctionAsync(auction);
            await unit.SaveChangesAsync();

        }
    }
}
