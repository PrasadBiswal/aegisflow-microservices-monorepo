using AegisFlow.Notifications.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AegisFlow.Notifications.Api.Data;

// notifications-api owns the NotificationsDb database. No other service reads or writes it directly.
public sealed class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : DbContext(options)
{
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(notification =>
        {
            notification.ToTable("Notifications");
            notification.HasKey(n => n.Id);
            notification.HasIndex(n => n.Recipient);
            notification.Property(n => n.Channel).HasConversion<string>().HasMaxLength(10);
            notification.Property(n => n.Status).HasConversion<string>().HasMaxLength(10);
            notification.Property(n => n.Recipient).HasMaxLength(Notification.RecipientMaxLength).IsRequired();
            notification.Property(n => n.Subject).HasMaxLength(Notification.SubjectMaxLength);
            notification.Property(n => n.Body).HasMaxLength(Notification.BodyMaxLength).IsRequired();
        });
    }
}
