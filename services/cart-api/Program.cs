using AegisFlow.Cart.Api.Data;
using AegisFlow.Cart.Api.Endpoints;
using AegisFlow.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();                        // logging, errors, health checks, Swagger
builder.AddSqlServerDatabase<CartDbContext>();       // SQL Server + /health/ready database check

var app = builder.Build();

app.MapServiceDefaults();                            // /, /health/live, /health/ready, /swagger
app.MapCartEndpoints();                              // /api/carts/{userId}

await app.RunOrMigrateAsync<CartDbContext>(args);    // "migrate" argument = apply migrations and exit

// Lets the test project start this API in memory (WebApplicationFactory<Program>).
public partial class Program { }
