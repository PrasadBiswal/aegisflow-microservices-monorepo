using AegisFlow.Catalog.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AegisFlow.Catalog.Api.Data;

// catalog-api owns the CatalogDb database. No other service reads or writes it directly.
public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(product =>
        {
            product.ToTable("Products");
            product.HasKey(p => p.Id);
            product.Property(p => p.Sku).HasMaxLength(Product.SkuMaxLength).IsRequired();
            product.HasIndex(p => p.Sku).IsUnique();
            product.Property(p => p.Name).HasMaxLength(Product.NameMaxLength).IsRequired();
            product.Property(p => p.Description).HasMaxLength(Product.DescriptionMaxLength);
            product.Property(p => p.Price).HasPrecision(18, 2);
        });
    }
}
