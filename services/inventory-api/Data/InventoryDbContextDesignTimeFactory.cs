using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AegisFlow.Inventory.Api.Data;

// Used only by 'dotnet ef migrations add', which never opens a connection,
// so a placeholder connection string is enough and no password is needed.
public sealed class InventoryDbContextDesignTimeFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseSqlServer("Server=localhost;Database=InventoryDb;")
            .Options;

        return new InventoryDbContext(options);
    }
}
