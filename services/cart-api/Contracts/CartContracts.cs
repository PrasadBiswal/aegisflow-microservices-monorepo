using AegisFlow.Cart.Api.Models;
using AegisFlow.ServiceDefaults;

namespace AegisFlow.Cart.Api.Contracts;

public sealed record AddCartItemRequest(Guid ProductId, int Quantity, decimal UnitPrice)
{
    public ValidationErrors Validate() => new ValidationErrors()
        .Rule(ProductId != Guid.Empty, "productId", "'productId' is required.")
        .Rule(Quantity is >= 1 and <= CartItem.MaxQuantity, "quantity", $"'quantity' must be between 1 and {CartItem.MaxQuantity}.")
        .Rule(UnitPrice > 0, "unitPrice", "'unitPrice' must be greater than 0.");
}

public sealed record UpdateCartItemRequest(int Quantity)
{
    public ValidationErrors Validate() => new ValidationErrors()
        .Rule(Quantity is >= 1 and <= CartItem.MaxQuantity, "quantity", $"'quantity' must be between 1 and {CartItem.MaxQuantity}.");
}

public sealed record CartItemResponse(Guid ProductId, int Quantity, decimal UnitPrice, decimal LineTotal);

public sealed record CartResponse(Guid UserId, IReadOnlyList<CartItemResponse> Items, int ItemCount, decimal Total)
{
    public static CartResponse From(Guid userId, IReadOnlyList<CartItem> items)
    {
        var lines = items.Select(i => new CartItemResponse(i.ProductId, i.Quantity, i.UnitPrice, i.Quantity * i.UnitPrice)).ToList();
        return new CartResponse(userId, lines, lines.Sum(l => l.Quantity), lines.Sum(l => l.LineTotal));
    }
}
