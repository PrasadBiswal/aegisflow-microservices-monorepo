using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AegisFlow.ServiceDefaults;

public static class SqlServerExtensions
{
    // Registers the service's DbContext against SQL Server and adds a readiness check for it.
    public static IHostApplicationBuilder AddSqlServerDatabase<TContext>(this IHostApplicationBuilder builder)
        where TContext : DbContext
    {
        // The connection string is built when the DbContext is first needed (not at startup),
        // so tests can swap in another database without setting DB_* variables.
        builder.Services.AddDbContext<TContext>((services, options) =>
        {
            var configuration = services.GetRequiredService<IConfiguration>();
            options.UseSqlServer(
                SqlServerConnection.Build(configuration),
                sql => sql.EnableRetryOnFailure());
        });

        builder.Services.AddHealthChecks()
            .AddCheck<DbContextHealthCheck<TContext>>("database", tags: new[] { ServiceDefaultsExtensions.ReadyTag });

        return builder;
    }
}
