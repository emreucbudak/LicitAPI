using FlashMediator;
using Licit.AuctionService.Application.Interface;
using Licit.AuctionService.Application.Repository;

namespace Licit.AuctionService.Application.Feature.CQRS.Auction.Command.CreateAuction
{
    public class CreateAuctionCommandHandler(IAuctionRepository repo,IUnitOfWork unit) : IRequestHandler<CreateAuctionCommandRequest>
    {
        public async  Task Handle(CreateAuctionCommandRequest request, CancellationToken cancellationToken)
        {
            var auction = new Domain.Entities.Auction()
            {
                Description = request.Description,
                EndDate = request.EndDate,
                StartDate = request.StartDate,
                AuctionName = request.AuctionName,
                CreatedByUserId = request.CreatedByUserId,
                ImgUrls = request.ImgUrls,
                IncreaseAmount = request.IncreaseAmount,
                StartPrice = request.StartPrice,
                Status = request.Status,
                Rules = request.Rules,

            };
            await repo.CreateAuctionAsync(auction);
            await unit.SaveChangesAsync();

        }
    }
}
