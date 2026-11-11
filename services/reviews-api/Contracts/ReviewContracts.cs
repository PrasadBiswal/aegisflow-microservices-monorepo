using AegisFlow.Reviews.Api.Models;
using AegisFlow.ServiceDefaults;

namespace AegisFlow.Reviews.Api.Contracts;

public sealed record CreateReviewRequest(Guid ProductId, Guid UserId, int Rating, string? Title, string? Comment)
{
    public ValidationErrors Validate() => new ValidationErrors()
        .Rule(ProductId != Guid.Empty, "productId", "'productId' is required.")
        .Rule(UserId != Guid.Empty, "userId", "'userId' is required.")
        .Rule(Rating is >= 1 and <= 5, "rating", "'rating' must be between 1 and 5.")
        .Required("title", Title, Review.TitleMaxLength)
        .MaxLength("comment", Comment, Review.CommentMaxLength);
}

public sealed record ReviewResponse(Guid Id, Guid ProductId, Guid UserId, int Rating, string Title, string? Comment, DateTime CreatedAtUtc)
{
    public static ReviewResponse From(Review r) => new(r.Id, r.ProductId, r.UserId, r.Rating, r.Title, r.Comment, r.CreatedAtUtc);
}

public sealed record RatingSummaryResponse(Guid ProductId, int ReviewCount, double AverageRating);
