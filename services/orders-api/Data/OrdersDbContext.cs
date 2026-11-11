using AegisFlow.Orders.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AegisFlow.Orders.Api.Data;

// orders-api owns the OrdersDb database. No other service reads or writes it directly.
public sealed class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(order =>
        {
            order.ToTable("Orders");
            order.HasKey(o => o.Id);
            order.HasIndex(o => o.UserId);
            order.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
            order.Property(o => o.TotalAmount).HasPrecision(18, 2);
            order.HasMany(o => o.Lines).WithOne().HasForeignKey(l => l.OrderId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderLine>(line =>
        {
            line.ToTable("OrderLines");
            line.HasKey(l => l.Id);
            line.Property(l => l.UnitPrice).HasPrecision(18, 2);
        });
    }
}
