using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AegisFlow.ServiceDefaults;

// Shared setup for every AegisFlow service, so all 10 services behave the same way.
public static class ServiceDefaultsExtensions
{
    // Health checks with this tag run on /health/ready (e.g. the database check).
    public const string ReadyTag = "ready";

    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        // Structured JSON logs, so CloudWatch (or any log platform) can search them by field.
        builder.Logging.ClearProviders();
        builder.Logging.AddJsonConsole(options =>
        {
            options.UseUtcTimestamp = true;
            options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
        });

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddProblemDetails();
        builder.Services.AddHealthChecks();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        return builder;
    }

    public static WebApplication MapServiceDefaults(this WebApplication app)
    {
        // Unhandled errors return a generic RFC 7807 response; the details go to the log only.
        app.UseExceptionHandler();
        app.UseStatusCodePages();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        var serviceName = app.Configuration["SERVICE_NAME"] ?? app.Environment.ApplicationName;
        var environmentName = app.Environment.EnvironmentName;

        app.MapGet("/", (TimeProvider clock) =>
                TypedResults.Ok(new ServiceInfo(serviceName, environmentName, clock.GetUtcNow())))
            .ExcludeFromDescription();

        // Liveness: is the process running? Runs no checks, so a slow database never restarts the pod.
        app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });

        // Readiness: can the service do its job? Runs the checks tagged "ready".
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains(ReadyTag),
        });

        return app;
    }
}

public sealed record ServiceInfo(string Service, string Environment, DateTimeOffset TimestampUtc);
