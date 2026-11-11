using System.Net;
using System.Net.Http.Json;
using AegisFlow.Inventory.Api.Contracts;

namespace AegisFlow.Inventory.Api.Tests;

public sealed class StockEndpointsTests(InventoryApiFactory factory) : IClassFixture<InventoryApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Stock_can_be_set_and_adjusted()
    {
        var productId = Guid.NewGuid();
        await _client.PutAsJsonAsync($"/api/inventory/{productId}", new SetStockRequest(10));

        var response = await _client.PostAsJsonAsync($"/api/inventory/{productId}/adjust", new AdjustStockRequest(-3));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var stock = await response.Content.ReadFromJsonAsync<StockResponse>();
        Assert.NotNull(stock);
        Assert.Equal(7, stock.QuantityOnHand);
    }

    [Fact]
    public async Task Stock_cannot_go_below_zero()
    {
        var productId = Guid.NewGuid();
        await _client.PutAsJsonAsync($"/api/inventory/{productId}", new SetStockRequest(2));

        var response = await _client.PostAsJsonAsync($"/api/inventory/{productId}/adjust", new AdjustStockRequest(-5));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Negative_quantity_is_rejected()
    {
        var response = await _client.PutAsJsonAsync($"/api/inventory/{Guid.NewGuid()}", new SetStockRequest(-1));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
