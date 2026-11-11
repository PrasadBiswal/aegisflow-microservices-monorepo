using AegisFlow.Reporting.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AegisFlow.Reporting.Api.Data;

// reporting-api owns the ReportingDb database. No other service reads or writes it directly.
public sealed class ReportingDbContext(DbContextOptions<ReportingDbContext> options) : DbContext(options)
{
    public DbSet<SaleRecord> SaleRecords => Set<SaleRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SaleRecord>(sale =>
        {
            sale.ToTable("SaleRecords");
            sale.HasKey(s => s.OrderId);                       // one record per order = no double counting
            sale.Property(s => s.OrderId).ValueGeneratedNever();
            sale.HasIndex(s => s.OccurredAtUtc);
            sale.Property(s => s.Amount).HasPrecision(18, 2);
        });
    }
}
