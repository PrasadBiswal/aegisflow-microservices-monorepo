using AegisFlow.Users.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AegisFlow.Users.Api.Data;

// users-api owns the UsersDb database. No other service reads or writes it directly.
public sealed class UsersDbContext(DbContextOptions<UsersDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(user =>
        {
            user.ToTable("Users");
            user.HasKey(u => u.Id);
            user.Property(u => u.Email).HasMaxLength(User.EmailMaxLength).IsRequired();
            user.HasIndex(u => u.Email).IsUnique();
            user.Property(u => u.FullName).HasMaxLength(User.FullNameMaxLength).IsRequired();
        });
    }
}
