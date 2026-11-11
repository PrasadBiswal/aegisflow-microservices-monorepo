using System.Net;
using System.Net.Http.Json;
using AegisFlow.ServiceDefaults;

namespace AegisFlow.Reviews.Api.Tests;

public sealed class HealthEndpointsTests(ReviewsApiFactory factory) : IClassFixture<ReviewsApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task Health_endpoints_return_ok(string path)
    {
        var response = await _client.GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Root_endpoint_identifies_the_service()
    {
        var info = await _client.GetFromJsonAsync<ServiceInfo>("/");

        Assert.NotNull(info);
        Assert.Equal("reviews-api", info.Service);
    }
}
