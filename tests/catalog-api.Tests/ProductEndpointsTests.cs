using System.Net;
using System.Net.Http.Json;
using AegisFlow.Catalog.Api.Contracts;

namespace AegisFlow.Catalog.Api.Tests;

public sealed class ProductEndpointsTests(CatalogApiFactory factory) : IClassFixture<CatalogApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Created_product_can_be_read_back()
    {
        var response = await _client.PostAsJsonAsync("/api/products", new CreateProductRequest(UniqueSku(), "Keyboard", null, 49.99m));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(created);
        var fetched = await _client.GetFromJsonAsync<ProductResponse>($"/api/products/{created.Id}");
        Assert.NotNull(fetched);
        Assert.Equal(created.Sku, fetched.Sku);
    }

    [Fact]
    public async Task Duplicate_sku_returns_conflict()
    {
        var request = new CreateProductRequest(UniqueSku(), "Mouse", null, 19.99m);
        await _client.PostAsJsonAsync("/api/products", request);

        var duplicate = await _client.PostAsJsonAsync("/api/products", request);

        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
    }

    [Fact]
    public async Task Invalid_product_returns_bad_request()
    {
        var response = await _client.PostAsJsonAsync("/api/products", new CreateProductRequest("", "", null, 0m));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static string UniqueSku() => $"SKU-{Guid.NewGuid():N}"[..20];
}
