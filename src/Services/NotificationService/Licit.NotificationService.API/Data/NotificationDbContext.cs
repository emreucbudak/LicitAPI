using Licit.NotificationService.API.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Licit.NotificationService.API.Data;

public sealed class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public DbSet<NotificationItem> Notifications => Set<NotificationItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<NotificationItem>(entity =>
        {
            entity.ToTable("Notifications");
            entity.HasKey(notification => notification.Id);

            entity.Property(notification => notification.Id)
                .HasMaxLength(32)
                .IsRequired();

            entity.Property(notification => notification.UserId)
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(notification => notification.Type)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(notification => notification.Title)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(notification => notification.Body)
                .HasMaxLength(2000);

            entity.Property(notification => notification.LinkUrl)
                .HasMaxLength(1000);

            entity.Property(notification => notification.Data)
                .HasColumnType("jsonb");

            entity.Property(notification => notification.CreatedAt)
                .IsRequired();

            entity.HasIndex(notification => new { notification.UserId, notification.CreatedAt });
            entity.HasIndex(notification => new { notification.UserId, notification.ReadAt });
        });
    }
}
