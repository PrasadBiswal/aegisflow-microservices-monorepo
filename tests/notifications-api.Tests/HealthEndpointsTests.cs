using System.Net;
using System.Net.Http.Json;
using AegisFlow.ServiceDefaults;

namespace AegisFlow.Notifications.Api.Tests;

public sealed class HealthEndpointsTests(NotificationsApiFactory factory) : IClassFixture<NotificationsApiFactory>
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
        Assert.Equal("notifications-api", info.Service);
    }
}
