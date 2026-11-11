using AegisFlow.Catalog.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AegisFlow.Catalog.Api.Tests;

// Starts catalog-api in memory and replaces SQL Server with an in-memory database,
// so these tests run anywhere (laptop or CI) without Docker or DB_* variables.
public sealed class CatalogApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"catalog-api-tests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<CatalogDbContext>>();
            services.AddDbContext<CatalogDbContext>(options => options.UseInMemoryDatabase(_databaseName));
        });
    }
}
