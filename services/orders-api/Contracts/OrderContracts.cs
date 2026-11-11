using AegisFlow.Orders.Api.Models;
using AegisFlow.ServiceDefaults;

namespace AegisFlow.Orders.Api.Contracts;

public sealed record OrderLineRequest(Guid ProductId, int Quantity, decimal UnitPrice);

public sealed record CreateOrderRequest(Guid UserId, IReadOnlyList<OrderLineRequest>? Lines)
{
    public const int MaxLines = 100;

    public ValidationErrors Validate()
    {
        var errors = new ValidationErrors()
            .Rule(UserId != Guid.Empty, "userId", "'userId' is required.")
            .Rule(Lines is { Count: > 0 and <= MaxLines }, "lines", $"An order needs 1 to {MaxLines} lines.");

        foreach (var (line, index) in (Lines ?? []).Select((l, i) => (l, i)))
        {
            errors.Rule(line.ProductId != Guid.Empty, $"lines[{index}].productId", "'productId' is required.")
                  .Rule(line.Quantity > 0, $"lines[{index}].quantity", "'quantity' must be greater than 0.")
                  .Rule(line.UnitPrice > 0, $"lines[{index}].unitPrice", "'unitPrice' must be greater than 0.");
        }

        return errors;
    }
}

public sealed record ChangeOrderStatusRequest(string? Status)
{
    public ValidationErrors Validate() => new ValidationErrors().OneOf<OrderStatus>("status", Status);
}

public sealed record OrderLineResponse(Guid ProductId, int Quantity, decimal UnitPrice, decimal LineTotal);

public sealed record OrderResponse(
    Guid Id, Guid UserId, string Status, decimal TotalAmount, IReadOnlyList<OrderLineResponse> Lines,
    DateTime CreatedAtUtc, DateTime? UpdatedAtUtc)
{
    public static OrderResponse From(Order o) => new(
        o.Id, o.UserId, o.Status.ToString(), o.TotalAmount,
        o.Lines.Select(l => new OrderLineResponse(l.ProductId, l.Quantity, l.UnitPrice, l.Quantity * l.UnitPrice)).ToList(),
        o.CreatedAtUtc, o.UpdatedAtUtc);
}

public sealed record OrderSummaryResponse(Guid Id, Guid UserId, string Status, decimal TotalAmount, DateTime CreatedAtUtc)
{
    public static OrderSummaryResponse From(Order o) => new(o.Id, o.UserId, o.Status.ToString(), o.TotalAmount, o.CreatedAtUtc);
}
