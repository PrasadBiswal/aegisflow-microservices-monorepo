using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AegisFlow.Shipping.Api.Data;

// Used only by 'dotnet ef migrations add', which never opens a connection,
// so a placeholder connection string is enough and no password is needed.
public sealed class ShippingDbContextDesignTimeFactory : IDesignTimeDbContextFactory<ShippingDbContext>
{
    public ShippingDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ShippingDbContext>()
            .UseSqlServer("Server=localhost;Database=ShippingDb;")
            .Options;

        return new ShippingDbContext(options);
    }
}
