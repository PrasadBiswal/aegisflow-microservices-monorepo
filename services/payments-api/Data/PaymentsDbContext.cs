using AegisFlow.Payments.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AegisFlow.Payments.Api.Data;

// payments-api owns the PaymentsDb database. No other service reads or writes it directly.
public sealed class PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : DbContext(options)
{
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(payment =>
        {
            payment.ToTable("Payments");
            payment.HasKey(p => p.Id);
            payment.HasIndex(p => p.OrderId);
            payment.Property(p => p.Amount).HasPrecision(18, 2);
            payment.Property(p => p.Currency).HasMaxLength(Payment.CurrencyLength).IsRequired();
            payment.Property(p => p.Method).HasConversion<string>().HasMaxLength(20);
            payment.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
        });
    }
}
