using Licit.TenderingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licit.TenderingService.Infrastructure.Data.Configurations;

public class TenderRuleConfiguration : IEntityTypeConfiguration<TenderRule>
{
    public void Configure(EntityTypeBuilder<TenderRule> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.Description)
            .HasMaxLength(1000)
            .IsRequired();
    }
}
