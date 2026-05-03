namespace Licit.AuctionService.Application.Repository
{
    public interface IAuctionRepository
    {
         Task<Domain.Entities.Auction> GetAuctionByIdAsync(Guid auctionId);
         Task UpdateAuctionAsync(Domain.Entities.Auction auction);
        Task<IEnumerable<Domain.Entities.Auction>> GetActiveAuctions();
        Task CreateAuctionAsync (Domain.Entities.Auction auction);

    }
}
