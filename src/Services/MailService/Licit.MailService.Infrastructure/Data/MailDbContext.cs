using Licit.MailService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Licit.MailService.Infrastructure.Data;

public class MailDbContext : DbContext
{
    public MailDbContext(DbContextOptions<MailDbContext> options) : base(options) { }

    public DbSet<EmailMessage> EmailMessages => Set<EmailMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MailDbContext).Assembly);
    }
}
