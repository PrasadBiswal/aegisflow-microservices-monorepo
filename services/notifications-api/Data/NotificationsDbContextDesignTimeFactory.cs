using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AegisFlow.Notifications.Api.Data;

// Used only by 'dotnet ef migrations add', which never opens a connection,
// so a placeholder connection string is enough and no password is needed.
public sealed class NotificationsDbContextDesignTimeFactory : IDesignTimeDbContextFactory<NotificationsDbContext>
{
    public NotificationsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<NotificationsDbContext>()
            .UseSqlServer("Server=localhost;Database=NotificationsDb;")
            .Options;

        return new NotificationsDbContext(options);
    }
}
