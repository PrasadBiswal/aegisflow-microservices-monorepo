using AegisFlow.Notifications.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AegisFlow.Notifications.Api.Tests;

// Starts notifications-api in memory and replaces SQL Server with an in-memory database,
// so these tests run anywhere (laptop or CI) without Docker or DB_* variables.
public sealed class NotificationsApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"notifications-api-tests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<NotificationsDbContext>>();
            services.AddDbContext<NotificationsDbContext>(options => options.UseInMemoryDatabase(_databaseName));
        });
    }
}
