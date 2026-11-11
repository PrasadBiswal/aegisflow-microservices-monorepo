using AegisFlow.Notifications.Api.Data;
using AegisFlow.Notifications.Api.Endpoints;
using AegisFlow.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();                                // logging, errors, health checks, Swagger
builder.AddSqlServerDatabase<NotificationsDbContext>();      // SQL Server + /health/ready database check

var app = builder.Build();

app.MapServiceDefaults();                                    // /, /health/live, /health/ready, /swagger
app.MapNotificationEndpoints();                              // /api/notifications

await app.RunOrMigrateAsync<NotificationsDbContext>(args);   // "migrate" argument = apply migrations and exit

// Lets the test project start this API in memory (WebApplicationFactory<Program>).
public partial class Program { }
