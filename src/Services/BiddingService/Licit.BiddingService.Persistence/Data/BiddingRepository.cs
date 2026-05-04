using Licit.BiddingService.Application.Repository;
using Licit.BiddingService.Domain.Entities;

namespace Licit.BiddingService.Persistence.Data
{
    public class BiddingRepository(BiddingDbContext db) : IBiddingRepository
    {
        public async Task CreateBid(Bid bid, CancellationToken cancellationToken = default)
        {
            await db.Bids.AddAsync(bid, cancellationToken);
        }
    }
}
