using AegisFlow.Inventory.Api.Contracts;
using AegisFlow.Inventory.Api.Data;
using AegisFlow.Inventory.Api.Models;
using AegisFlow.ServiceDefaults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace AegisFlow.Inventory.Api.Endpoints;

public static class StockEndpoints
{
    public static IEndpointRouteBuilder MapStockEndpoints(this IEndpointRouteBuilder app)
    {
        var stock = app.MapGroup("/api/inventory").WithTags("Inventory");
        stock.MapGet("/", List).WithName("ListStock");
        stock.MapGet("/{productId:guid}", Get).WithName("GetStock");
        stock.MapPut("/{productId:guid}", Set).WithName("SetStock");
        stock.MapPost("/{productId:guid}/adjust", Adjust).WithName("AdjustStock");
        return app;
    }

    private static async Task<Ok<PagedResponse<StockResponse>>> List(
        InventoryDbContext db, CancellationToken ct, int page = 1, int pageSize = 20)
    {
        var result = await db.StockItems.AsNoTracking().OrderBy(s => s.ProductId).ToPagedResponseAsync(page, pageSize, ct);
        return TypedResults.Ok(result.Map(StockResponse.From));
    }

    private static async Task<Results<Ok<StockResponse>, NotFound>> Get(Guid productId, InventoryDbContext db, CancellationToken ct)
    {
        var item = await db.StockItems.AsNoTracking().FirstOrDefaultAsync(s => s.ProductId == productId, ct);
        return item is null ? TypedResults.NotFound() : TypedResults.Ok(StockResponse.From(item));
    }

    // PUT sets the absolute quantity (e.g. after a stock count) and creates the record if needed.
    private static async Task<Results<Ok<StockResponse>, ValidationProblem>> Set(
        Guid productId, SetStockRequest request, InventoryDbContext db, TimeProvider clock, CancellationToken ct)
    {
        var errors = request.Validate();
        if (errors.HasErrors) return TypedResults.ValidationProblem(errors.ToDictionary());

        var item = await db.StockItems.FirstOrDefaultAsync(s => s.ProductId == productId, ct);
        if (item is null)
        {
            item = new StockItem { ProductId = productId };
            db.StockItems.Add(item);
        }

        item.QuantityOnHand = request.Quantity;
        item.UpdatedAtUtc = clock.GetUtcNow().UtcDateTime;
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(StockResponse.From(item));
    }

    // POST .../adjust changes stock by a delta and never lets it go below zero.
    private static async Task<Results<Ok<StockResponse>, NotFound, ValidationProblem, ProblemHttpResult>> Adjust(
        Guid productId, AdjustStockRequest request, InventoryDbContext db, TimeProvider clock, CancellationToken ct)
    {
        var errors = request.Validate();
        if (errors.HasErrors) return TypedResults.ValidationProblem(errors.ToDictionary());

        var item = await db.StockItems.FirstOrDefaultAsync(s => s.ProductId == productId, ct);
        if (item is null) return TypedResults.NotFound();

        if (item.QuantityOnHand + request.Delta < 0)
            return TypedResults.Problem(title: "Insufficient stock.", detail: $"Only {item.QuantityOnHand} in stock.", statusCode: StatusCodes.Status409Conflict);

        item.QuantityOnHand += request.Delta;
        item.UpdatedAtUtc = clock.GetUtcNow().UtcDateTime;
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(StockResponse.From(item));
    }
}
