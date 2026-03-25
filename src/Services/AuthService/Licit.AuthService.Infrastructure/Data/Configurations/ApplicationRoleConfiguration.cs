using Licit.AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licit.AuthService.Infrastructure.Data.Configurations;

public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public static readonly Guid AdminRoleId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
    public static readonly Guid UserRoleId = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901");

    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.Property(e => e.Description).HasMaxLength(256);

        builder.HasData(
            new ApplicationRole
            {
                Id = AdminRoleId,
                Name = "Admin",
                NormalizedName = "ADMIN",
                Description = "Sistem yöneticisi rolü",
                ConcurrencyStamp = AdminRoleId.ToString()
            },
            new ApplicationRole
            {
                Id = UserRoleId,
                Name = "User",
                NormalizedName = "USER",
                Description = "Standart kullanıcı rolü",
                ConcurrencyStamp = UserRoleId.ToString()
            }
        );
    }
}
