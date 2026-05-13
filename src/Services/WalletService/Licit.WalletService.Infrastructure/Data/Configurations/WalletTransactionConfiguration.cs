using Licit.WalletService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licit.WalletService.Infrastructure.Data.Configurations;

public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
{
    public void Configure(EntityTypeBuilder<WalletTransaction> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedNever();

        builder.Property(t => t.Type)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(t => t.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.HasIndex(t => new { t.WalletId, t.CreatedAt })
            .IsDescending(false, true);

        builder.HasIndex(t => new { t.WalletId, t.Type, t.ReferenceId })
            .IsUnique();
    }
}
