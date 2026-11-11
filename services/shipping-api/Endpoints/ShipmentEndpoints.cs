using AegisFlow.ServiceDefaults;
using AegisFlow.Shipping.Api.Contracts;
using AegisFlow.Shipping.Api.Data;
using AegisFlow.Shipping.Api.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace AegisFlow.Shipping.Api.Endpoints;

public static class ShipmentEndpoints
{
    public static IEndpointRouteBuilder MapShipmentEndpoints(this IEndpointRouteBuilder app)
    {
        var shipments = app.MapGroup("/api/shipments").WithTags("Shipments");
        shipments.MapGet("/", List).WithName("ListShipments");
        shipments.MapGet("/{id:guid}", Get).WithName("GetShipment");
        shipments.MapPost("/", Create).WithName("CreateShipment");
        shipments.MapPost("/{id:guid}/ship", Ship).WithName("ShipShipment");
        shipments.MapPost("/{id:guid}/deliver", Deliver).WithName("DeliverShipment");
        return app;
    }

    // GET /api/shipments?orderId=...&page=1&pageSize=20
    private static async Task<Ok<PagedResponse<ShipmentResponse>>> List(
        ShippingDbContext db, CancellationToken ct, Guid? orderId = null, int page = 1, int pageSize = 20)
    {
        var query = db.Shipments.AsNoTracking();
        if (orderId is not null) query = query.Where(s => s.OrderId == orderId);

        var result = await query.OrderByDescending(s => s.CreatedAtUtc).ThenBy(s => s.Id).ToPagedResponseAsync(page, pageSize, ct);
        return TypedResults.Ok(result.Map(ShipmentResponse.From));
    }

    private static async Task<Results<Ok<ShipmentResponse>, NotFound>> Get(Guid id, ShippingDbContext db, CancellationToken ct)
    {
        var shipment = await db.Shipments.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, ct);
        return shipment is null ? TypedResults.NotFound() : TypedResults.Ok(ShipmentResponse.From(shipment));
    }

    private static async Task<Results<Created<ShipmentResponse>, ValidationProblem, ProblemHttpResult>> Create(
        CreateShipmentRequest request, ShippingDbContext db, TimeProvider clock, CancellationToken ct)
    {
        var errors = request.Validate();
        if (errors.HasErrors) return TypedResults.ValidationProblem(errors.ToDictionary());

        // One shipment per order.
        if (await db.Shipments.AnyAsync(s => s.OrderId == request.OrderId, ct))
            return TypedResults.Problem(title: "This order already has a shipment.", statusCode: StatusCodes.Status409Conflict);

        var shipment = new Shipment
        {
            OrderId = request.OrderId,
            Carrier = request.Carrier!.Trim(),
            Address = request.Address!.Trim(),
            CreatedAtUtc = clock.GetUtcNow().UtcDateTime,
        };
        db.Shipments.Add(shipment);
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/shipments/{shipment.Id}", ShipmentResponse.From(shipment));
    }

    // Pending -> Shipped (needs a tracking number).
    private static async Task<Results<Ok<ShipmentResponse>, NotFound, ValidationProblem, ProblemHttpResult>> Ship(
        Guid id, ShipShipmentRequest request, ShippingDbContext db, TimeProvider clock, CancellationToken ct)
    {
        var errors = request.Validate();
        if (errors.HasErrors) return TypedResults.ValidationProblem(errors.ToDictionary());

        var shipment = await db.Shipments.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (shipment is null) return TypedResults.NotFound();
        if (shipment.Status != ShipmentStatus.Pending) return Conflict(shipment.Status, ShipmentStatus.Shipped);

        shipment.Status = ShipmentStatus.Shipped;
        shipment.TrackingNumber = request.TrackingNumber!.Trim();
        shipment.ShippedAtUtc = clock.GetUtcNow().UtcDateTime;
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(ShipmentResponse.From(shipment));
    }

    // Shipped -> Delivered.
    private static async Task<Results<Ok<ShipmentResponse>, NotFound, ProblemHttpResult>> Deliver(
        Guid id, ShippingDbContext db, TimeProvider clock, CancellationToken ct)
    {
        var shipment = await db.Shipments.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (shipment is null) return TypedResults.NotFound();
        if (shipment.Status != ShipmentStatus.Shipped) return Conflict(shipment.Status, ShipmentStatus.Delivered);

        shipment.Status = ShipmentStatus.Delivered;
        shipment.DeliveredAtUtc = clock.GetUtcNow().UtcDateTime;
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(ShipmentResponse.From(shipment));
    }

    private static ProblemHttpResult Conflict(ShipmentStatus from, ShipmentStatus to) =>
        TypedResults.Problem(title: $"Cannot change shipment status from {from} to {to}.", statusCode: StatusCodes.Status409Conflict);
}
