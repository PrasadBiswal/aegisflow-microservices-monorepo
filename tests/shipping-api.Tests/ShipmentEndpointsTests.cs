using System.Net;
using System.Net.Http.Json;
using AegisFlow.Shipping.Api.Contracts;

namespace AegisFlow.Shipping.Api.Tests;

public sealed class ShipmentEndpointsTests(ShippingApiFactory factory) : IClassFixture<ShippingApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Shipment_moves_from_pending_to_shipped_to_delivered()
    {
        var shipment = await CreateShipmentAsync(Guid.NewGuid());

        var ship = await _client.PostAsJsonAsync($"/api/shipments/{shipment.Id}/ship", new ShipShipmentRequest("TRK123"));
        var deliver = await _client.PostAsync($"/api/shipments/{shipment.Id}/deliver", null);

        Assert.Equal(HttpStatusCode.OK, ship.StatusCode);
        Assert.Equal(HttpStatusCode.OK, deliver.StatusCode);
        var final = await deliver.Content.ReadFromJsonAsync<ShipmentResponse>();
        Assert.Equal("Delivered", final?.Status);
    }

    [Fact]
    public async Task Pending_shipment_cannot_be_delivered()
    {
        var shipment = await CreateShipmentAsync(Guid.NewGuid());

        var deliver = await _client.PostAsync($"/api/shipments/{shipment.Id}/deliver", null);

        Assert.Equal(HttpStatusCode.Conflict, deliver.StatusCode);
    }

    [Fact]
    public async Task Second_shipment_for_the_same_order_is_rejected()
    {
        var orderId = Guid.NewGuid();
        await CreateShipmentAsync(orderId);

        var second = await _client.PostAsJsonAsync("/api/shipments", new CreateShipmentRequest(orderId, "DHL", "1 Main St"));

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    private async Task<ShipmentResponse> CreateShipmentAsync(Guid orderId)
    {
        var response = await _client.PostAsJsonAsync("/api/shipments", new CreateShipmentRequest(orderId, "DHL", "1 Main St"));
        response.EnsureSuccessStatusCode();
        var shipment = await response.Content.ReadFromJsonAsync<ShipmentResponse>();
        Assert.NotNull(shipment);
        return shipment;
    }
}
