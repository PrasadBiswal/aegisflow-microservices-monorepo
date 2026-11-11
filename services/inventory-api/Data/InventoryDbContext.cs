using AegisFlow.Inventory.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AegisFlow.Inventory.Api.Data;

// inventory-api owns the InventoryDb database. No other service reads or writes it directly.
public sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<StockItem> StockItems => Set<StockItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StockItem>(stock =>
        {
            stock.ToTable("StockItems");
            stock.HasKey(s => s.ProductId);
            stock.Property(s => s.ProductId).ValueGeneratedNever();
        });
    }
}
