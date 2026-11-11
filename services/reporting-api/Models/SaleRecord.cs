namespace AegisFlow.Reporting.Api.Models;

// One completed sale, sent to reporting-api (later via an event), so reports never query another service's database.
public sealed class SaleRecord
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public int ItemCount { get; set; }
    public DateTime OccurredAtUtc { get; set; }
}
