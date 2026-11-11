using AegisFlow.Users.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AegisFlow.Users.Api.Tests;

// Starts users-api in memory and replaces SQL Server with an in-memory database,
// so these tests run anywhere (laptop or CI) without Docker or DB_* variables.
public sealed class UsersApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"users-api-tests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<UsersDbContext>>();
            services.AddDbContext<UsersDbContext>(options => options.UseInMemoryDatabase(_databaseName));
        });
    }
}
