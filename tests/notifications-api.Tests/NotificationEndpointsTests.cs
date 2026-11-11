using System.Net;
using System.Net.Http.Json;
using AegisFlow.Notifications.Api.Contracts;

namespace AegisFlow.Notifications.Api.Tests;

public sealed class NotificationEndpointsTests(NotificationsApiFactory factory) : IClassFixture<NotificationsApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Queued_notification_can_be_sent_only_once()
    {
        var create = await _client.PostAsJsonAsync("/api/notifications",
            new CreateNotificationRequest("Email", "ada@example.com", "Welcome", "Hello Ada"));
        Assert.Equal(HttpStatusCode.Accepted, create.StatusCode);
        var notification = await create.Content.ReadFromJsonAsync<NotificationResponse>();
        Assert.NotNull(notification);

        var first = await _client.PostAsync($"/api/notifications/{notification.Id}/send", null);
        var second = await _client.PostAsync($"/api/notifications/{notification.Id}/send", null);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Theory]
    [InlineData("Email", "ada@example.com", null)]   // email without subject
    [InlineData("Sms", "12345", null)]               // invalid phone number
    [InlineData("Fax", "ada@example.com", "Hi")]     // unknown channel
    public async Task Invalid_notification_is_rejected(string channel, string recipient, string? subject)
    {
        var response = await _client.PostAsJsonAsync("/api/notifications",
            new CreateNotificationRequest(channel, recipient, subject, "Hello"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
