namespace AegisFlow.Cart.Api.Models;

// One product line in a user's cart. UnitPrice is the price when the item was added.
public sealed class CartItem
{
    public const int MaxQuantity = 999;

    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTime AddedAtUtc { get; set; }
}
