using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AegisFlow.ServiceDefaults;

public static class MigrationExtensions
{
    public const string MigrateCommand = "migrate";

    // Normal start: runs the web API.
    // Started with the "migrate" argument: applies EF Core migrations once, then exits.
    // Compose (users-api-migrate) and Kubernetes (a Job) run "migrate" as a separate one-off step,
    // so several replicas never race to change the database at the same time.
    public static async Task RunOrMigrateAsync<TContext>(this WebApplication app, string[] args)
        where TContext : DbContext
    {
        if (!args.Contains(MigrateCommand, StringComparer.OrdinalIgnoreCase))
        {
            await app.RunAsync();
            return;
        }

        var contextName = typeof(TContext).Name;
        try
        {
            await using var scope = app.Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();

            app.Logger.LogInformation("Applying database migrations for {DbContext}", contextName);
            await dbContext.Database.MigrateAsync();
            app.Logger.LogInformation("Database migrations applied for {DbContext}", contextName);
        }
        catch (Exception ex)
        {
            app.Logger.LogCritical(ex, "Database migrations failed for {DbContext}", contextName);
            Environment.ExitCode = 1; // non-zero exit = failed step in Compose / Kubernetes
        }
        finally
        {
            // Flushes the log queue before the process exits.
            await app.DisposeAsync();
        }
    }
}
