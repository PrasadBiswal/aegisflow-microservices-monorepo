using AegisFlow.Inventory.Api.Data;
using AegisFlow.Inventory.Api.Endpoints;
using AegisFlow.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();                            // logging, errors, health checks, Swagger
builder.AddSqlServerDatabase<InventoryDbContext>();      // SQL Server + /health/ready database check

var app = builder.Build();

app.MapServiceDefaults();                                // /, /health/live, /health/ready, /swagger
app.MapStockEndpoints();                                 // /api/inventory

await app.RunOrMigrateAsync<InventoryDbContext>(args);   // "migrate" argument = apply migrations and exit

// Lets the test project start this API in memory (WebApplicationFactory<Program>).
public partial class Program { }
