using Licit.BiddingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Licit.BiddingService.Persistence.Data
{
    public class BiddingDbContext(DbContextOptions<BiddingDbContext> options) : DbContext(options)
    {
        public DbSet<Bid> Bids => Set<Bid>();
        public DbSet<AuctionBidState> AuctionBidStates => Set<AuctionBidState>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Bid>(entity =>
            {
                entity.HasKey(bid => bid.Id);

                entity.Property(bid => bid.IdempotencyKey)
                    .IsRequired()
                    .HasMaxLength(160);

                entity.HasIndex(bid => bid.AuctionId);
                entity.HasIndex(bid => new { bid.BidderUserId, bid.IdempotencyKey })
                    .IsUnique();
            });

            modelBuilder.Entity<AuctionBidState>(entity =>
            {
                entity.HasKey(state => state.Id);
                entity.HasIndex(state => state.AuctionId)
                    .IsUnique();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
