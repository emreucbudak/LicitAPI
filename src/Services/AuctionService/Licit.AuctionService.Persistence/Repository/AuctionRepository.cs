using Licit.AuctionService.Application.Repository;
using Licit.AuctionService.Domain.Entities;
using Licit.AuctionService.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Licit.AuctionService.Persistence.Repository
{
    public class AuctionRepository(AuctionDbContext dbContext) : IAuctionRepository
    {
        private  DbSet<Auction> _dbSet => dbContext.Set<Auction>();

        public async Task CreateAuctionAsync(Auction auction)
        {
            await _dbSet.AddAsync(auction);
        }

        public async Task<IEnumerable<Auction>> GetActiveAuctions()
        {
            return await _dbSet.Where(a => a.Status == AuctionStatus.Aktif).ToListAsync();
        }

        public async  Task<Auction> GetAuctionByIdAsync(Guid auctionId)
        {
            return await _dbSet.FirstOrDefaultAsync(a => a.Id == auctionId);
        }
        public async Task UpdateAuctionAsync(Auction auction)
        {
            _dbSet.Update(auction);
        }
    }
}
