using AegisFlow.Catalog.Api.Data;
using AegisFlow.Catalog.Api.Endpoints;
using AegisFlow.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();                          // logging, errors, health checks, Swagger
builder.AddSqlServerDatabase<CatalogDbContext>();      // SQL Server + /health/ready database check

var app = builder.Build();

app.MapServiceDefaults();                              // /, /health/live, /health/ready, /swagger
app.MapProductEndpoints();                             // /api/products

await app.RunOrMigrateAsync<CatalogDbContext>(args);   // "migrate" argument = apply migrations and exit

// Lets the test project start this API in memory (WebApplicationFactory<Program>).
public partial class Program { }
