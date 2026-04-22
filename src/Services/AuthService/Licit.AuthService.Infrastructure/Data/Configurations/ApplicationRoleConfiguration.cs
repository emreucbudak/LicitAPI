using Licit.AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licit.AuthService.Infrastructure.Data.Configurations;

public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public static readonly Guid AdminRoleId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
    public static readonly Guid UserRoleId = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901");
    private static readonly DateTime RoleSeedBaseCreatedAt = new(2026, 4, 20, 10, 34, 0, 6, DateTimeKind.Utc);

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
                ConcurrencyStamp = AdminRoleId.ToString(),
                CreatedAt = RoleSeedBaseCreatedAt.AddTicks(743)
            },
            new ApplicationRole
            {
                Id = UserRoleId,
                Name = "User",
                NormalizedName = "USER",
                Description = "Standart kullanıcı rolü",
                ConcurrencyStamp = UserRoleId.ToString(),
                CreatedAt = RoleSeedBaseCreatedAt.AddTicks(7855)
            }
        );
    }
}
