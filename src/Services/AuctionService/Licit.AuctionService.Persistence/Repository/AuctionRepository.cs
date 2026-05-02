using Licit.AuctionService.Application.Repository;
using Licit.AuctionService.Domain.Entities;
using Licit.AuctionService.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Licit.AuctionService.Persistence.Repository
{
    public class AuctionRepository(AuctionDbContext dbContext) : IAuctionRepository
    {
        private  DbSet<Auction> _dbSet => dbContext.Set<Auction>();
        public async  Task<Auction> GetAuctionByIdAsync(Guid auctionId)
        {
            if (auctionId == Guid.Empty)
            {
                throw new ArgumentException("Auction Id Boş Olamaz", nameof(auctionId));
            }
            return await _dbSet.FirstOrDefaultAsync(a => a.Id == auctionId);
        }

        public async Task<bool> IsAuctionActive(Guid auctionId)
        {
            if (auctionId == Guid.Empty)
            {
                throw new ArgumentException("Auction Id Boş Olamaz", nameof(auctionId));
            }
            Auction auc = await _dbSet.FirstOrDefaultAsync(a => a.Id == auctionId);
            if (auc is null)
            {
                throw new InvalidOperationException("Auction bulunamadı");
            }
            return auc.Status == AuctionStatus.Aktif;
        }

        public async Task UpdateAuctionAsync(Auction auction)
        {
            _dbSet.Update(auction);
        }
    }
}
