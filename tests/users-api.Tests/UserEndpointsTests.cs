using System.Net;
using System.Net.Http.Json;
using AegisFlow.Users.Api.Contracts;

namespace AegisFlow.Users.Api.Tests;

// Calls the real HTTP endpoints of users-api running in memory (see UsersApiFactory).
public sealed class UserEndpointsTests(UsersApiFactory factory) : IClassFixture<UsersApiFactory>
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
    public async Task Created_user_can_be_read_back()
    {
        var request = new CreateUserRequest(UniqueEmail(), "Ada Lovelace");

        var createResponse = await _client.PostAsJsonAsync("/api/users", request);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(created);
        Assert.Equal(request.Email, created.Email);

        var fetched = await _client.GetFromJsonAsync<UserResponse>($"/api/users/{created.Id}");
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched.Id);
    }

    [Fact]
    public async Task Creating_a_user_with_an_existing_email_returns_conflict()
    {
        var request = new CreateUserRequest(UniqueEmail(), "Ada Lovelace");
        await _client.PostAsJsonAsync("/api/users", request);

        var duplicateResponse = await _client.PostAsJsonAsync("/api/users", request);

        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
    }

    [Fact]
    public async Task Creating_a_user_with_invalid_data_returns_bad_request()
    {
        var response = await _client.PostAsJsonAsync("/api/users", new CreateUserRequest("not-an-email", ""));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Unknown_user_returns_not_found()
    {
        var response = await _client.GetAsync($"/api/users/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Deleted_user_is_no_longer_found()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/users", new CreateUserRequest(UniqueEmail(), "Grace Hopper"));
        var created = await createResponse.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(created);

        var deleteResponse = await _client.DeleteAsync($"/api/users/{created.Id}");
        var getResponse = await _client.GetAsync($"/api/users/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    // Each test uses its own email, because all tests in this class share one in-memory database.
    private static string UniqueEmail() => $"user-{Guid.NewGuid():N}@example.com";
}
