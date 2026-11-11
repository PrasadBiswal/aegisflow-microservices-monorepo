using AegisFlow.ServiceDefaults;
using AegisFlow.Shipping.Api.Data;
using AegisFlow.Shipping.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();                           // logging, errors, health checks, Swagger
builder.AddSqlServerDatabase<ShippingDbContext>();      // SQL Server + /health/ready database check

var app = builder.Build();

app.MapServiceDefaults();                               // /, /health/live, /health/ready, /swagger
app.MapShipmentEndpoints();                             // /api/shipments

await app.RunOrMigrateAsync<ShippingDbContext>(args);   // "migrate" argument = apply migrations and exit

// Lets the test project start this API in memory (WebApplicationFactory<Program>).
public partial class Program { }
