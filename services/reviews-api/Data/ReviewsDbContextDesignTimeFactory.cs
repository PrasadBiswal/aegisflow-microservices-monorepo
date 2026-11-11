using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AegisFlow.Reviews.Api.Data;

// Used only by 'dotnet ef migrations add', which never opens a connection,
// so a placeholder connection string is enough and no password is needed.
public sealed class ReviewsDbContextDesignTimeFactory : IDesignTimeDbContextFactory<ReviewsDbContext>
{
    public ReviewsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ReviewsDbContext>()
            .UseSqlServer("Server=localhost;Database=ReviewsDb;")
            .Options;

        return new ReviewsDbContext(options);
    }
}
