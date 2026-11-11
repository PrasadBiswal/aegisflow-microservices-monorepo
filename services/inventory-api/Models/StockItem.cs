namespace AegisFlow.Inventory.Api.Models;

// Stock level for one product. ProductId comes from catalog-api (no cross-database foreign key).
public sealed class StockItem
{
    public Guid ProductId { get; set; }
    public int QuantityOnHand { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
