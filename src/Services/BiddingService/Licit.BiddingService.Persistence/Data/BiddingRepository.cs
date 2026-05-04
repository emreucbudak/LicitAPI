using Licit.BiddingService.Application.Repository;
using Licit.BiddingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Licit.BiddingService.Persistence.Data
{
    public class BiddingRepository(BiddingDbContext db) : IBiddingRepository
    {
        public async Task CreateBid(Bid bid, CancellationToken cancellationToken = default)
        {
            await db.Bids.AddAsync(bid, cancellationToken);
        }

        public async Task<IReadOnlyList<Guid>> GetDistinctBidderUserIdsForAuctionAsync(
            Guid auctionId,
            Guid excludingBidderUserId,
            CancellationToken cancellationToken = default)
        {
            return await db.Bids
                .AsNoTracking()
                .Where(bid => bid.AuctionId == auctionId && bid.BidderUserId != excludingBidderUserId)
                .Select(bid => bid.BidderUserId)
                .Distinct()
                .ToListAsync(cancellationToken);
        }
    }
}
