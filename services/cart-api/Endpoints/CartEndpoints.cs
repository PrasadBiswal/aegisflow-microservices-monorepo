using AegisFlow.Cart.Api.Contracts;
using AegisFlow.Cart.Api.Data;
using AegisFlow.Cart.Api.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace AegisFlow.Cart.Api.Endpoints;

public static class CartEndpoints
{
    public static IEndpointRouteBuilder MapCartEndpoints(this IEndpointRouteBuilder app)
    {
        var carts = app.MapGroup("/api/carts/{userId:guid}").WithTags("Carts");
        carts.MapGet("/", Get).WithName("GetCart");
        carts.MapPost("/items", AddItem).WithName("AddCartItem");
        carts.MapPut("/items/{productId:guid}", UpdateItem).WithName("UpdateCartItem");
        carts.MapDelete("/items/{productId:guid}", RemoveItem).WithName("RemoveCartItem");
        carts.MapDelete("/", Clear).WithName("ClearCart");
        return app;
    }

    // A user with no items gets an empty cart (not 404).
    private static async Task<Ok<CartResponse>> Get(Guid userId, CartDbContext db, CancellationToken ct) =>
        TypedResults.Ok(CartResponse.From(userId, await LoadItemsAsync(db, userId, ct)));

    private static async Task<Results<Ok<CartResponse>, ValidationProblem>> AddItem(
        Guid userId, AddCartItemRequest request, CartDbContext db, TimeProvider clock, CancellationToken ct)
    {
        var errors = request.Validate();
        if (errors.HasErrors) return TypedResults.ValidationProblem(errors.ToDictionary());

        // Adding a product that is already in the cart increases its quantity.
        var item = await db.CartItems.FirstOrDefaultAsync(i => i.UserId == userId && i.ProductId == request.ProductId, ct);
        if (item is null)
        {
            db.CartItems.Add(new CartItem
            {
                UserId = userId,
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                UnitPrice = request.UnitPrice,
                AddedAtUtc = clock.GetUtcNow().UtcDateTime,
            });
        }
        else
        {
            item.Quantity = Math.Min(item.Quantity + request.Quantity, CartItem.MaxQuantity);
            item.UnitPrice = request.UnitPrice;
        }

        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(CartResponse.From(userId, await LoadItemsAsync(db, userId, ct)));
    }

    private static async Task<Results<Ok<CartResponse>, NotFound, ValidationProblem>> UpdateItem(
        Guid userId, Guid productId, UpdateCartItemRequest request, CartDbContext db, CancellationToken ct)
    {
        var errors = request.Validate();
        if (errors.HasErrors) return TypedResults.ValidationProblem(errors.ToDictionary());

        var item = await db.CartItems.FirstOrDefaultAsync(i => i.UserId == userId && i.ProductId == productId, ct);
        if (item is null) return TypedResults.NotFound();

        item.Quantity = request.Quantity;
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(CartResponse.From(userId, await LoadItemsAsync(db, userId, ct)));
    }

    private static async Task<Results<NoContent, NotFound>> RemoveItem(Guid userId, Guid productId, CartDbContext db, CancellationToken ct)
    {
        var item = await db.CartItems.FirstOrDefaultAsync(i => i.UserId == userId && i.ProductId == productId, ct);
        if (item is null) return TypedResults.NotFound();

        db.CartItems.Remove(item);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> Clear(Guid userId, CartDbContext db, CancellationToken ct)
    {
        db.CartItems.RemoveRange(await db.CartItems.Where(i => i.UserId == userId).ToListAsync(ct));
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static Task<List<CartItem>> LoadItemsAsync(CartDbContext db, Guid userId, CancellationToken ct) =>
        db.CartItems.AsNoTracking().Where(i => i.UserId == userId).OrderBy(i => i.AddedAtUtc).ToListAsync(ct);
}
