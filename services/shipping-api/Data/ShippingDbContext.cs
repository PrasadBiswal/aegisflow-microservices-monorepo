using AegisFlow.Shipping.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AegisFlow.Shipping.Api.Data;

// shipping-api owns the ShippingDb database. No other service reads or writes it directly.
public sealed class ShippingDbContext(DbContextOptions<ShippingDbContext> options) : DbContext(options)
{
    public DbSet<Shipment> Shipments => Set<Shipment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Shipment>(shipment =>
        {
            shipment.ToTable("Shipments");
            shipment.HasKey(s => s.Id);
            shipment.HasIndex(s => s.OrderId).IsUnique();
            shipment.Property(s => s.Carrier).HasMaxLength(Shipment.CarrierMaxLength).IsRequired();
            shipment.Property(s => s.Address).HasMaxLength(Shipment.AddressMaxLength).IsRequired();
            shipment.Property(s => s.TrackingNumber).HasMaxLength(Shipment.TrackingNumberMaxLength);
            shipment.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
        });
    }
}
