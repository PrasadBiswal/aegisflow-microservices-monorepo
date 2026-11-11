using System.Net;
using System.Net.Http.Json;
using AegisFlow.Cart.Api.Contracts;

namespace AegisFlow.Cart.Api.Tests;

public sealed class CartEndpointsTests(CartApiFactory factory) : IClassFixture<CartApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Adding_the_same_product_twice_increases_quantity_and_total()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        await _client.PostAsJsonAsync($"/api/carts/{userId}/items", new AddCartItemRequest(productId, 1, 10m));
        await _client.PostAsJsonAsync($"/api/carts/{userId}/items", new AddCartItemRequest(productId, 2, 10m));

        var cart = await _client.GetFromJsonAsync<CartResponse>($"/api/carts/{userId}");

        Assert.NotNull(cart);
        var item = Assert.Single(cart.Items);
        Assert.Equal(3, item.Quantity);
        Assert.Equal(30m, cart.Total);
    }

    [Fact]
    public async Task Invalid_quantity_is_rejected()
    {
        var response = await _client.PostAsJsonAsync($"/api/carts/{Guid.NewGuid()}/items", new AddCartItemRequest(Guid.NewGuid(), 0, 10m));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
