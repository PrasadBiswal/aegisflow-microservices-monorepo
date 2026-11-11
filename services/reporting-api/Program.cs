using AegisFlow.Reporting.Api.Data;
using AegisFlow.Reporting.Api.Endpoints;
using AegisFlow.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();                            // logging, errors, health checks, Swagger
builder.AddSqlServerDatabase<ReportingDbContext>();      // SQL Server + /health/ready database check

var app = builder.Build();

app.MapServiceDefaults();                                // /, /health/live, /health/ready, /swagger
app.MapReportEndpoints();                                // /api/reports/sales

await app.RunOrMigrateAsync<ReportingDbContext>(args);   // "migrate" argument = apply migrations and exit

// Lets the test project start this API in memory (WebApplicationFactory<Program>).
public partial class Program { }
