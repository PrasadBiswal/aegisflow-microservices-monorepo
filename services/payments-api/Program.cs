using AegisFlow.Payments.Api.Data;
using AegisFlow.Payments.Api.Endpoints;
using AegisFlow.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();                           // logging, errors, health checks, Swagger
builder.AddSqlServerDatabase<PaymentsDbContext>();      // SQL Server + /health/ready database check

var app = builder.Build();

app.MapServiceDefaults();                               // /, /health/live, /health/ready, /swagger
app.MapPaymentEndpoints();                              // /api/payments

await app.RunOrMigrateAsync<PaymentsDbContext>(args);   // "migrate" argument = apply migrations and exit

// Lets the test project start this API in memory (WebApplicationFactory<Program>).
public partial class Program { }
