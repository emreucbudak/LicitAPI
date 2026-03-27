using Licit.TenderingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Licit.TenderingService.Infrastructure.Data;

public class TenderingDbContext : DbContext
{
    public TenderingDbContext(DbContextOptions<TenderingDbContext> options) : base(options) { }

    public DbSet<Tender> Tenders => Set<Tender>();
    public DbSet<TenderRule> TenderRules => Set<TenderRule>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TenderingDbContext).Assembly);
    }
}
