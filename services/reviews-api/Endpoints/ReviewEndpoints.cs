using AegisFlow.Reviews.Api.Contracts;
using AegisFlow.Reviews.Api.Data;
using AegisFlow.Reviews.Api.Models;
using AegisFlow.ServiceDefaults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace AegisFlow.Reviews.Api.Endpoints;

public static class ReviewEndpoints
{
    public static IEndpointRouteBuilder MapReviewEndpoints(this IEndpointRouteBuilder app)
    {
        var reviews = app.MapGroup("/api/reviews").WithTags("Reviews");
        reviews.MapGet("/", List).WithName("ListReviews");
        reviews.MapGet("/{id:guid}", Get).WithName("GetReview");
        reviews.MapGet("/summary/{productId:guid}", Summary).WithName("GetRatingSummary");
        reviews.MapPost("/", Create).WithName("CreateReview");
        reviews.MapDelete("/{id:guid}", Delete).WithName("DeleteReview");
        return app;
    }

    // GET /api/reviews?productId=...&page=1&pageSize=20 (newest first)
    private static async Task<Ok<PagedResponse<ReviewResponse>>> List(
        ReviewsDbContext db, CancellationToken ct, Guid? productId = null, int page = 1, int pageSize = 20)
    {
        var query = db.Reviews.AsNoTracking();
        if (productId is not null) query = query.Where(r => r.ProductId == productId);

        var result = await query.OrderByDescending(r => r.CreatedAtUtc).ThenBy(r => r.Id).ToPagedResponseAsync(page, pageSize, ct);
        return TypedResults.Ok(result.Map(ReviewResponse.From));
    }

    private static async Task<Results<Ok<ReviewResponse>, NotFound>> Get(Guid id, ReviewsDbContext db, CancellationToken ct)
    {
        var review = await db.Reviews.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, ct);
        return review is null ? TypedResults.NotFound() : TypedResults.Ok(ReviewResponse.From(review));
    }

    // Review count and average rating for one product (0 when there are no reviews).
    private static async Task<Ok<RatingSummaryResponse>> Summary(Guid productId, ReviewsDbContext db, CancellationToken ct)
    {
        var ratings = db.Reviews.Where(r => r.ProductId == productId).Select(r => r.Rating);
        var count = await ratings.CountAsync(ct);
        var average = count == 0 ? 0 : await ratings.AverageAsync(ct);
        return TypedResults.Ok(new RatingSummaryResponse(productId, count, Math.Round(average, 2)));
    }

    private static async Task<Results<Created<ReviewResponse>, ValidationProblem, ProblemHttpResult>> Create(
        CreateReviewRequest request, ReviewsDbContext db, TimeProvider clock, CancellationToken ct)
    {
        var errors = request.Validate();
        if (errors.HasErrors) return TypedResults.ValidationProblem(errors.ToDictionary());

        if (await db.Reviews.AnyAsync(r => r.ProductId == request.ProductId && r.UserId == request.UserId, ct))
            return TypedResults.Problem(title: "This user has already reviewed this product.", statusCode: StatusCodes.Status409Conflict);

        var review = new Review
        {
            ProductId = request.ProductId,
            UserId = request.UserId,
            Rating = request.Rating,
            Title = request.Title!.Trim(),
            Comment = request.Comment?.Trim(),
            CreatedAtUtc = clock.GetUtcNow().UtcDateTime,
        };
        db.Reviews.Add(review);
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/reviews/{review.Id}", ReviewResponse.From(review));
    }

    private static async Task<Results<NoContent, NotFound>> Delete(Guid id, ReviewsDbContext db, CancellationToken ct)
    {
        var review = await db.Reviews.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (review is null) return TypedResults.NotFound();

        db.Reviews.Remove(review);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }
}
