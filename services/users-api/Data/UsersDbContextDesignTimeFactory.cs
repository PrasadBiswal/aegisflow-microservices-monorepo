using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AegisFlow.Users.Api.Data;

// Used only by the 'dotnet ef' tool when you create a migration.
// 'dotnet ef migrations add' never opens a connection, so a placeholder connection string
// is enough and no password is needed. Migrations are applied with the "migrate" command instead.
public sealed class UsersDbContextDesignTimeFactory : IDesignTimeDbContextFactory<UsersDbContext>
{
    public UsersDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseSqlServer("Server=localhost;Database=UsersDb;")
            .Options;

        return new UsersDbContext(options);
    }
}
