using AegisFlow.Cart.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AegisFlow.Cart.Api.Data;

// cart-api owns the CartDb database. No other service reads or writes it directly.
public sealed class CartDbContext(DbContextOptions<CartDbContext> options) : DbContext(options)
{
    public DbSet<CartItem> CartItems => Set<CartItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CartItem>(item =>
        {
            item.ToTable("CartItems");
            item.HasKey(i => i.Id);
            item.HasIndex(i => new { i.UserId, i.ProductId }).IsUnique();
            item.Property(i => i.UnitPrice).HasPrecision(18, 2);
        });
    }
}
