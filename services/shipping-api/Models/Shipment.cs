namespace AegisFlow.Shipping.Api.Models;

public enum ShipmentStatus { Pending, Shipped, Delivered }

public sealed class Shipment
{
    public const int CarrierMaxLength = 100;
    public const int AddressMaxLength = 500;
    public const int TrackingNumberMaxLength = 100;

    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string Carrier { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? TrackingNumber { get; set; }
    public ShipmentStatus Status { get; set; } = ShipmentStatus.Pending;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ShippedAtUtc { get; set; }
    public DateTime? DeliveredAtUtc { get; set; }
}
