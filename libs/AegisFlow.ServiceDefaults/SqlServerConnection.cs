using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AegisFlow.ServiceDefaults;

// Builds the SQL Server connection string from environment variables.
// The same variable names are used by Docker Compose (local) and Helm (Kubernetes).
public static class SqlServerConnection
{
    public static string Build(IConfiguration configuration)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = $"{configuration["DB_SERVER"] ?? "localhost"},{configuration["DB_PORT"] ?? "1433"}",
            InitialCatalog = Require(configuration, "DB_NAME"),
            UserID = configuration["DB_USER"] ?? "sa",
            Password = Require(configuration, "DB_PASSWORD"),
            Encrypt = SqlConnectionEncryptOption.Mandatory,
            // "true" only for local containers with a self-signed certificate.
            // In AWS leave it unset (false), so the RDS certificate is validated.
            TrustServerCertificate = configuration.GetValue<bool>("DB_TRUST_SERVER_CERTIFICATE"),
            ConnectTimeout = 15,
        };

        return builder.ConnectionString;
    }

    // Fails fast with a clear message instead of trying to connect with missing settings.
    private static string Require(IConfiguration configuration, string key)
    {
        var value = configuration[key];
        return string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException($"Required setting '{key}' is not set.")
            : value;
    }
}
