using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AegisFlow.Cart.Api.Data;

// Used only by 'dotnet ef migrations add', which never opens a connection,
// so a placeholder connection string is enough and no password is needed.
public sealed class CartDbContextDesignTimeFactory : IDesignTimeDbContextFactory<CartDbContext>
{
    public CartDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<CartDbContext>()
            .UseSqlServer("Server=localhost;Database=CartDb;")
            .Options;

        return new CartDbContext(options);
    }
}
