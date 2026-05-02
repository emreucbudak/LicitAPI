using Microsoft.EntityFrameworkCore;

namespace Licit.AuctionService.Persistence.Data
{
    public class AuctionDbContext : DbContext
    {
        public AuctionDbContext(DbContextOptions options) : base(options)
        {
        }

        protected AuctionDbContext()
        {
        }
        public DbSet<Domain.Entities.Auction> Auctions { get; set; } 
        public DbSet<Domain.Entities.AuctionStatus> AuctionStatuses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
