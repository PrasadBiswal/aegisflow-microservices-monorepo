using System.Net;
using System.Net.Http.Json;
using AegisFlow.Reviews.Api.Contracts;

namespace AegisFlow.Reviews.Api.Tests;

public sealed class ReviewEndpointsTests(ReviewsApiFactory factory) : IClassFixture<ReviewsApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Summary_averages_the_ratings_of_a_product()
    {
        var productId = Guid.NewGuid();
        await _client.PostAsJsonAsync("/api/reviews", new CreateReviewRequest(productId, Guid.NewGuid(), 4, "Good", null));
        await _client.PostAsJsonAsync("/api/reviews", new CreateReviewRequest(productId, Guid.NewGuid(), 5, "Great", null));

        var summary = await _client.GetFromJsonAsync<RatingSummaryResponse>($"/api/reviews/summary/{productId}");

        Assert.NotNull(summary);
        Assert.Equal(2, summary.ReviewCount);
        Assert.Equal(4.5, summary.AverageRating);
    }

    [Fact]
    public async Task Second_review_by_the_same_user_is_rejected()
    {
        var request = new CreateReviewRequest(Guid.NewGuid(), Guid.NewGuid(), 3, "Okay", null);
        await _client.PostAsJsonAsync("/api/reviews", request);

        var second = await _client.PostAsJsonAsync("/api/reviews", request);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Rating_outside_1_to_5_is_rejected()
    {
        var response = await _client.PostAsJsonAsync("/api/reviews", new CreateReviewRequest(Guid.NewGuid(), Guid.NewGuid(), 6, "Wow", null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
