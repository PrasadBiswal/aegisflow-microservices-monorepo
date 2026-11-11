using AegisFlow.Inventory.Api.Models;
using AegisFlow.ServiceDefaults;

namespace AegisFlow.Inventory.Api.Contracts;

public sealed record SetStockRequest(int Quantity)
{
    public ValidationErrors Validate() => new ValidationErrors()
        .Rule(Quantity >= 0, "quantity", "'quantity' cannot be negative.");
}

// Positive delta = goods received; negative delta = goods removed (e.g. reserved for an order).
public sealed record AdjustStockRequest(int Delta)
{
    public ValidationErrors Validate() => new ValidationErrors()
        .Rule(Delta != 0, "delta", "'delta' must not be 0.");
}

public sealed record StockResponse(Guid ProductId, int QuantityOnHand, DateTime UpdatedAtUtc)
{
    public static StockResponse From(StockItem s) => new(s.ProductId, s.QuantityOnHand, s.UpdatedAtUtc);
}
