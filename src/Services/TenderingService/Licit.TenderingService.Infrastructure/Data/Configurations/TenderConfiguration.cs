using Licit.TenderingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licit.TenderingService.Infrastructure.Data.Configurations;

public class TenderConfiguration : IEntityTypeConfiguration<Tender>
{
    public void Configure(EntityTypeBuilder<Tender> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(t => t.StartingPrice)
            .HasPrecision(18, 2);

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(t => t.ImageUrl)
            .HasMaxLength(2048);

        builder.Property(t => t.ImageBlobName)
            .HasMaxLength(512);

        builder.HasMany(t => t.Rules)
            .WithOne(r => r.Tender)
            .HasForeignKey(r => r.TenderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => new { t.Status, t.EndDate, t.StartDate });
        builder.HasIndex(t => t.CreatedAt)
            .IsDescending();
        builder.HasIndex(t => new { t.CreatedByUserId, t.CreatedAt })
            .IsDescending(false, true);
        builder.HasIndex(t => t.EndDate);
        builder.HasIndex(t => t.StartDate);
        builder.HasIndex(t => t.Title);
        builder.HasIndex(t => t.Description);
        builder.HasIndex(t => new { t.Status, t.Title });
        builder.HasIndex(t => new { t.Status, t.EndDate, t.Title });
        builder.HasIndex(t => t.CategoryId);
    }
}
