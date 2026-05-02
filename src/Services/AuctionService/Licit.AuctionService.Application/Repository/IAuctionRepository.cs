namespace Licit.AuctionService.Application.Repository
{
    public interface IAuctionRepository
    {
        Task<bool> IsAuctionActive (Guid auctionId);
         Task<Domain.Entities.Auction> GetAuctionByIdAsync(Guid auctionId);
         Task UpdateAuctionAsync(Domain.Entities.Auction auction);

    }
}
