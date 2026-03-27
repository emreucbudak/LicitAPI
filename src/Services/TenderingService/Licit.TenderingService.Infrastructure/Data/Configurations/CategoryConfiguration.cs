using Licit.TenderingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licit.TenderingService.Infrastructure.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Tenders)
            .WithOne(t => t.Category)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.Name);
        builder.HasIndex(c => c.ParentCategoryId);

        // Seed Data
        var elektronik = new { Id = Guid.Parse("a1b2c3d4-0001-0001-0001-000000000001"), Name = "Elektronik", ParentCategoryId = (Guid?)null, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null };
        var otomobil = new { Id = Guid.Parse("a1b2c3d4-0001-0001-0002-000000000001"), Name = "Otomobil", ParentCategoryId = (Guid?)null, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null };
        var kitap = new { Id = Guid.Parse("a1b2c3d4-0001-0001-0003-000000000001"), Name = "Kitap", ParentCategoryId = (Guid?)null, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null };
        var giyim = new { Id = Guid.Parse("a1b2c3d4-0001-0001-0004-000000000001"), Name = "Giyim", ParentCategoryId = (Guid?)null, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null };
        var evMobilya = new { Id = Guid.Parse("a1b2c3d4-0001-0001-0005-000000000001"), Name = "Ev & Mobilya", ParentCategoryId = (Guid?)null, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null };
        var spor = new { Id = Guid.Parse("a1b2c3d4-0001-0001-0006-000000000001"), Name = "Spor & Outdoor", ParentCategoryId = (Guid?)null, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null };
        var insaatMalzeme = new { Id = Guid.Parse("a1b2c3d4-0001-0001-0007-000000000001"), Name = "İnşaat Malzemesi", ParentCategoryId = (Guid?)null, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null };
        var tarim = new { Id = Guid.Parse("a1b2c3d4-0001-0001-0008-000000000001"), Name = "Tarım & Hayvancılık", ParentCategoryId = (Guid?)null, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null };
        var saglik = new { Id = Guid.Parse("a1b2c3d4-0001-0001-0009-000000000001"), Name = "Sağlık & Medikal", ParentCategoryId = (Guid?)null, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null };
        var gida = new { Id = Guid.Parse("a1b2c3d4-0001-0001-0010-000000000001"), Name = "Gıda", ParentCategoryId = (Guid?)null, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null };

        // Alt kategoriler - Elektronik
        var telefon = new { Id = Guid.Parse("a1b2c3d4-0002-0001-0001-000000000001"), Name = "Telefon", ParentCategoryId = (Guid?)elektronik.Id, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null };
        var bilgisayar = new { Id = Guid.Parse("a1b2c3d4-0002-0001-0002-000000000001"), Name = "Bilgisayar", ParentCategoryId = (Guid?)elektronik.Id, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null };
        var televizyon = new { Id = Guid.Parse("a1b2c3d4-0002-0001-0003-000000000001"), Name = "Televizyon", ParentCategoryId = (Guid?)elektronik.Id, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null };

        // Alt kategoriler - Otomobil
        var binek = new { Id = Guid.Parse("a1b2c3d4-0002-0002-0001-000000000001"), Name = "Binek Araç", ParentCategoryId = (Guid?)otomobil.Id, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null };
        var ticari = new { Id = Guid.Parse("a1b2c3d4-0002-0002-0002-000000000001"), Name = "Ticari Araç", ParentCategoryId = (Guid?)otomobil.Id, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null };
        var yedekParca = new { Id = Guid.Parse("a1b2c3d4-0002-0002-0003-000000000001"), Name = "Yedek Parça", ParentCategoryId = (Guid?)otomobil.Id, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null };

        // Alt kategoriler - Giyim
        var erkekGiyim = new { Id = Guid.Parse("a1b2c3d4-0002-0004-0001-000000000001"), Name = "Erkek Giyim", ParentCategoryId = (Guid?)giyim.Id, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null };
        var kadinGiyim = new { Id = Guid.Parse("a1b2c3d4-0002-0004-0002-000000000001"), Name = "Kadın Giyim", ParentCategoryId = (Guid?)giyim.Id, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null };
        var ayakkabi = new { Id = Guid.Parse("a1b2c3d4-0002-0004-0003-000000000001"), Name = "Ayakkabı", ParentCategoryId = (Guid?)giyim.Id, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null };

        builder.HasData(
            elektronik, otomobil, kitap, giyim, evMobilya, spor, insaatMalzeme, tarim, saglik, gida,
            telefon, bilgisayar, televizyon,
            binek, ticari, yedekParca,
            erkekGiyim, kadinGiyim, ayakkabi
        );
    }
}
