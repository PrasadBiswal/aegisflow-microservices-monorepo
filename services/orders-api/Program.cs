using AegisFlow.Orders.Api.Data;
using AegisFlow.Orders.Api.Endpoints;
using AegisFlow.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();                         // logging, errors, health checks, Swagger
builder.AddSqlServerDatabase<OrdersDbContext>();      // SQL Server + /health/ready database check

var app = builder.Build();

app.MapServiceDefaults();                             // /, /health/live, /health/ready, /swagger
app.MapOrderEndpoints();                              // /api/orders

await app.RunOrMigrateAsync<OrdersDbContext>(args);   // "migrate" argument = apply migrations and exit

// Lets the test project start this API in memory (WebApplicationFactory<Program>).
public partial class Program { }
