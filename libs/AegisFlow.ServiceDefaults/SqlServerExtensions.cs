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
            // 'dotnet ef' (design time) never connects, so it gets a placeholder and needs no DB_* settings.
            var connectionString = EF.IsDesignTime
                ? "Server=localhost;Database=DesignTime;"
                : SqlServerConnection.Build(services.GetRequiredService<IConfiguration>());
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure());
        });

        builder.Services.AddHealthChecks()
            .AddCheck<DbContextHealthCheck<TContext>>("database", tags: new[] { ServiceDefaultsExtensions.ReadyTag });

        return builder;
    }
}
