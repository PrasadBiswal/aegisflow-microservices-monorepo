using AegisFlow.ServiceDefaults;
using AegisFlow.Users.Api.Data;
using AegisFlow.Users.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();                       // logging, errors, health checks, Swagger
builder.AddSqlServerDatabase<UsersDbContext>();     // SQL Server + /health/ready database check

var app = builder.Build();

app.MapServiceDefaults();                           // /, /health/live, /health/ready, /swagger
app.MapUserEndpoints();                             // /api/users

await app.RunOrMigrateAsync<UsersDbContext>(args);  // "migrate" argument = apply migrations and exit

// Lets the test project start this API in memory (WebApplicationFactory<Program>).
public partial class Program { }
