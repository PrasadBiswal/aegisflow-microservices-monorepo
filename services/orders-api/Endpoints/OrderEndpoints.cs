using AegisFlow.Orders.Api.Contracts;
using AegisFlow.Orders.Api.Data;
using AegisFlow.Orders.Api.Models;
using AegisFlow.ServiceDefaults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace AegisFlow.Orders.Api.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var orders = app.MapGroup("/api/orders").WithTags("Orders");
        orders.MapGet("/", List).WithName("ListOrders");
        orders.MapGet("/{id:guid}", Get).WithName("GetOrder");
        orders.MapPost("/", Create).WithName("CreateOrder");
        orders.MapPost("/{id:guid}/status", ChangeStatus).WithName("ChangeOrderStatus");
        return app;
    }

    // GET /api/orders?userId=...&page=1&pageSize=20 (newest first)
    private static async Task<Ok<PagedResponse<OrderSummaryResponse>>> List(
        OrdersDbContext db, CancellationToken ct, Guid? userId = null, int page = 1, int pageSize = 20)
    {
        var query = db.Orders.AsNoTracking();
        if (userId is not null) query = query.Where(o => o.UserId == userId);

        var result = await query.OrderByDescending(o => o.CreatedAtUtc).ThenBy(o => o.Id).ToPagedResponseAsync(page, pageSize, ct);
        return TypedResults.Ok(result.Map(OrderSummaryResponse.From));
    }

    private static async Task<Results<Ok<OrderResponse>, NotFound>> Get(Guid id, OrdersDbContext db, CancellationToken ct)
    {
        var order = await db.Orders.AsNoTracking().Include(o => o.Lines).FirstOrDefaultAsync(o => o.Id == id, ct);
        return order is null ? TypedResults.NotFound() : TypedResults.Ok(OrderResponse.From(order));
    }

    private static async Task<Results<Created<OrderResponse>, ValidationProblem>> Create(
        CreateOrderRequest request, OrdersDbContext db, TimeProvider clock, CancellationToken ct)
    {
        var errors = request.Validate();
        if (errors.HasErrors) return TypedResults.ValidationProblem(errors.ToDictionary());

        var order = new Order
        {
            UserId = request.UserId,
            CreatedAtUtc = clock.GetUtcNow().UtcDateTime,
            Lines = request.Lines!.Select(l => new OrderLine { ProductId = l.ProductId, Quantity = l.Quantity, UnitPrice = l.UnitPrice }).ToList(),
        };
        order.TotalAmount = order.Lines.Sum(l => l.Quantity * l.UnitPrice); // calculated by the server, never trusted from the client

        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/orders/{order.Id}", OrderResponse.From(order));
    }

    // POST /api/orders/{id}/status  { "status": "Paid" } — only allowed changes succeed (see Order.CanMoveTo).
    private static async Task<Results<Ok<OrderResponse>, NotFound, ValidationProblem, ProblemHttpResult>> ChangeStatus(
        Guid id, ChangeOrderStatusRequest request, OrdersDbContext db, TimeProvider clock, CancellationToken ct)
    {
        var errors = request.Validate();
        if (errors.HasErrors) return TypedResults.ValidationProblem(errors.ToDictionary());

        var order = await db.Orders.Include(o => o.Lines).FirstOrDefaultAsync(o => o.Id == id, ct);
        if (order is null) return TypedResults.NotFound();

        var next = Enum.Parse<OrderStatus>(request.Status!, ignoreCase: true);
        if (!order.CanMoveTo(next))
            return TypedResults.Problem(title: $"Cannot change order status from {order.Status} to {next}.", statusCode: StatusCodes.Status409Conflict);

        order.Status = next;
        order.UpdatedAtUtc = clock.GetUtcNow().UtcDateTime;
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(OrderResponse.From(order));
    }
}
