using Licit.WalletService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licit.WalletService.Infrastructure.Data.Configurations;

public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
{
    public void Configure(EntityTypeBuilder<WalletTransaction> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Type)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(t => t.Amount)
            .HasPrecision(18, 2);

        builder.Property(t => t.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(t => t.BalanceAfter)
            .HasPrecision(18, 2);

        builder.Property(t => t.FrozenBalanceAfter)
            .HasPrecision(18, 2);

        builder.HasIndex(t => t.WalletId);
        builder.HasIndex(t => t.CreatedAt);
    }
}
