using Licit.WalletService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licit.WalletService.Infrastructure.Data.Configurations;

public class WalletDepositPaymentConfiguration : IEntityTypeConfiguration<WalletDepositPayment>
{
    public void Configure(EntityTypeBuilder<WalletDepositPayment> builder)
    {
        builder.HasKey(payment => payment.Id);

        builder.Property(payment => payment.Amount)
            .HasPrecision(18, 2);

        builder.Property(payment => payment.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(payment => payment.Status)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(payment => payment.ClientIdempotencyKey)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(payment => payment.StripePaymentIntentId)
            .HasMaxLength(128);

        builder.Property(payment => payment.FailureCode)
            .HasMaxLength(128);

        builder.Property(payment => payment.FailureMessage)
            .HasMaxLength(500);

        builder.HasIndex(payment => new { payment.UserId, payment.ClientIdempotencyKey })
            .IsUnique();

        builder.HasIndex(payment => payment.StripePaymentIntentId)
            .IsUnique()
            .HasFilter("\"StripePaymentIntentId\" IS NOT NULL");
    }
}
