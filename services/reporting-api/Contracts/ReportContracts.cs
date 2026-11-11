using AegisFlow.Reporting.Api.Models;
using AegisFlow.ServiceDefaults;

namespace AegisFlow.Reporting.Api.Contracts;

public sealed record RecordSaleRequest(Guid OrderId, decimal Amount, int ItemCount, DateTime? OccurredAtUtc)
{
    public ValidationErrors Validate() => new ValidationErrors()
        .Rule(OrderId != Guid.Empty, "orderId", "'orderId' is required.")
        .Rule(Amount > 0, "amount", "'amount' must be greater than 0.")
        .Rule(ItemCount > 0, "itemCount", "'itemCount' must be greater than 0.");
}

public sealed record SaleResponse(Guid OrderId, decimal Amount, int ItemCount, DateTime OccurredAtUtc)
{
    public static SaleResponse From(SaleRecord s) => new(s.OrderId, s.Amount, s.ItemCount, s.OccurredAtUtc);
}

public sealed record DailySalesResponse(DateOnly Date, int OrderCount, int ItemCount, decimal Revenue);

public sealed record SalesSummaryResponse(
    DateOnly From, DateOnly To, int OrderCount, int ItemCount, decimal Revenue, decimal AverageOrderValue);
