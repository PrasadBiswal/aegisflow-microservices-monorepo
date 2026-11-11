using AegisFlow.Reviews.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AegisFlow.Reviews.Api.Tests;

// Starts reviews-api in memory and replaces SQL Server with an in-memory database,
// so these tests run anywhere (laptop or CI) without Docker or DB_* variables.
public sealed class ReviewsApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"reviews-api-tests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<ReviewsDbContext>>();
            services.AddDbContext<ReviewsDbContext>(options => options.UseInMemoryDatabase(_databaseName));
        });
    }
}
