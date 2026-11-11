using AegisFlow.Reviews.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AegisFlow.Reviews.Api.Data;

// reviews-api owns the ReviewsDb database. No other service reads or writes it directly.
public sealed class ReviewsDbContext(DbContextOptions<ReviewsDbContext> options) : DbContext(options)
{
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Review>(review =>
        {
            review.ToTable("Reviews");
            review.HasKey(r => r.Id);
            review.HasIndex(r => new { r.ProductId, r.UserId }).IsUnique();   // one review per user per product
            review.Property(r => r.Title).HasMaxLength(Review.TitleMaxLength).IsRequired();
            review.Property(r => r.Comment).HasMaxLength(Review.CommentMaxLength);
        });
    }
}
