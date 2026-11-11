using AegisFlow.ServiceDefaults;
using AegisFlow.Shipping.Api.Models;

namespace AegisFlow.Shipping.Api.Contracts;

public sealed record CreateShipmentRequest(Guid OrderId, string? Carrier, string? Address)
{
    public ValidationErrors Validate() => new ValidationErrors()
        .Rule(OrderId != Guid.Empty, "orderId", "'orderId' is required.")
        .Required("carrier", Carrier, Shipment.CarrierMaxLength)
        .Required("address", Address, Shipment.AddressMaxLength);
}

public sealed record ShipShipmentRequest(string? TrackingNumber)
{
    public ValidationErrors Validate() => new ValidationErrors()
        .Required("trackingNumber", TrackingNumber, Shipment.TrackingNumberMaxLength);
}

public sealed record ShipmentResponse(
    Guid Id, Guid OrderId, string Carrier, string Address, string? TrackingNumber, string Status,
    DateTime CreatedAtUtc, DateTime? ShippedAtUtc, DateTime? DeliveredAtUtc)
{
    public static ShipmentResponse From(Shipment s) =>
        new(s.Id, s.OrderId, s.Carrier, s.Address, s.TrackingNumber, s.Status.ToString(), s.CreatedAtUtc, s.ShippedAtUtc, s.DeliveredAtUtc);
}
