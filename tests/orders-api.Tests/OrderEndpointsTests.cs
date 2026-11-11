using System.Net;
using System.Net.Http.Json;
using AegisFlow.Orders.Api.Contracts;

namespace AegisFlow.Orders.Api.Tests;

public sealed class OrderEndpointsTests(OrdersApiFactory factory) : IClassFixture<OrdersApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Created_order_is_pending_with_server_calculated_total()
    {
        var order = await CreateOrderAsync();

        Assert.Equal(25m, order.TotalAmount);
        Assert.Equal("Pending", order.Status);
    }

    [Fact]
    public async Task Order_only_allows_valid_status_changes()
    {
        var order = await CreateOrderAsync();

        var shipBeforePaying = await _client.PostAsJsonAsync($"/api/orders/{order.Id}/status", new ChangeOrderStatusRequest("Shipped"));
        var pay = await _client.PostAsJsonAsync($"/api/orders/{order.Id}/status", new ChangeOrderStatusRequest("Paid"));

        Assert.Equal(HttpStatusCode.Conflict, shipBeforePaying.StatusCode);
        Assert.Equal(HttpStatusCode.OK, pay.StatusCode);
    }

    [Fact]
    public async Task Order_without_lines_is_rejected()
    {
        var response = await _client.PostAsJsonAsync("/api/orders", new CreateOrderRequest(Guid.NewGuid(), []));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<OrderResponse> CreateOrderAsync()
    {
        var request = new CreateOrderRequest(Guid.NewGuid(),
            [new OrderLineRequest(Guid.NewGuid(), 2, 10m), new OrderLineRequest(Guid.NewGuid(), 1, 5m)]);
        var response = await _client.PostAsJsonAsync("/api/orders", request);
        response.EnsureSuccessStatusCode();
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.NotNull(order);
        return order;
    }
}
