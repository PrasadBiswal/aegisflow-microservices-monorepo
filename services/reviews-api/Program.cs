using AegisFlow.Reviews.Api.Data;
using AegisFlow.Reviews.Api.Endpoints;
using AegisFlow.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();                          // logging, errors, health checks, Swagger
builder.AddSqlServerDatabase<ReviewsDbContext>();      // SQL Server + /health/ready database check

var app = builder.Build();

app.MapServiceDefaults();                              // /, /health/live, /health/ready, /swagger
app.MapReviewEndpoints();                              // /api/reviews

await app.RunOrMigrateAsync<ReviewsDbContext>(args);   // "migrate" argument = apply migrations and exit

// Lets the test project start this API in memory (WebApplicationFactory<Program>).
public partial class Program { }
