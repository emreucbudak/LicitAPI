using Licit.WalletService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licit.WalletService.Infrastructure.Data.Configurations;

public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.HasKey(w => w.Id);

        builder.HasIndex(w => w.UserId).IsUnique();

        builder.Property(w => w.Balance)
            .HasPrecision(18, 2);

        builder.Property(w => w.FrozenBalance)
            .HasPrecision(18, 2);

        builder.Property(w => w.RowVersion)
            .IsRowVersion();

        builder.HasMany(w => w.Transactions)
            .WithOne(t => t.Wallet)
            .HasForeignKey(t => t.WalletId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
