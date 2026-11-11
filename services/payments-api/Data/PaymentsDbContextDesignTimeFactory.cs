using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AegisFlow.Payments.Api.Data;

// Used only by 'dotnet ef migrations add', which never opens a connection,
// so a placeholder connection string is enough and no password is needed.
public sealed class PaymentsDbContextDesignTimeFactory : IDesignTimeDbContextFactory<PaymentsDbContext>
{
    public PaymentsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<PaymentsDbContext>()
            .UseSqlServer("Server=localhost;Database=PaymentsDb;")
            .Options;

        return new PaymentsDbContext(options);
    }
}
