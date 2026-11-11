using System.Net;
using System.Net.Http.Json;
using AegisFlow.Reporting.Api.Contracts;

namespace AegisFlow.Reporting.Api.Tests;

// Each test uses its own dates, because all tests in this class share one in-memory database.
public sealed class ReportEndpointsTests(ReportingApiFactory factory) : IClassFixture<ReportingApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Recording_the_same_order_twice_counts_it_once()
    {
        var sale = new RecordSaleRequest(Guid.NewGuid(), 100m, 2, new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc));

        var first = await _client.PostAsJsonAsync("/api/reports/sales", sale);
        var second = await _client.PostAsJsonAsync("/api/reports/sales", sale);
        var summary = await _client.GetFromJsonAsync<SalesSummaryResponse>("/api/reports/sales/summary?from=2025-01-15&to=2025-01-15");

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.NotNull(summary);
        Assert.Equal(1, summary.OrderCount);
        Assert.Equal(100m, summary.Revenue);
    }

    [Fact]
    public async Task Daily_report_groups_sales_by_day()
    {
        await _client.PostAsJsonAsync("/api/reports/sales", new RecordSaleRequest(Guid.NewGuid(), 10m, 1, new DateTime(2025, 2, 1, 9, 0, 0, DateTimeKind.Utc)));
        await _client.PostAsJsonAsync("/api/reports/sales", new RecordSaleRequest(Guid.NewGuid(), 20m, 1, new DateTime(2025, 2, 1, 18, 0, 0, DateTimeKind.Utc)));
        await _client.PostAsJsonAsync("/api/reports/sales", new RecordSaleRequest(Guid.NewGuid(), 5m, 1, new DateTime(2025, 2, 2, 12, 0, 0, DateTimeKind.Utc)));

        var days = await _client.GetFromJsonAsync<List<DailySalesResponse>>("/api/reports/sales/daily?from=2025-02-01&to=2025-02-02");

        Assert.NotNull(days);
        Assert.Collection(days,
            d => Assert.Equal(30m, d.Revenue),
            d => Assert.Equal(5m, d.Revenue));
    }

    [Fact]
    public async Task Range_where_from_is_after_to_is_rejected()
    {
        var response = await _client.GetAsync("/api/reports/sales/summary?from=2025-03-10&to=2025-03-01");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
