using AegisFlow.Catalog.Api.Contracts;
using AegisFlow.Catalog.Api.Data;
using AegisFlow.Catalog.Api.Models;
using AegisFlow.ServiceDefaults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace AegisFlow.Catalog.Api.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var products = app.MapGroup("/api/products").WithTags("Products");
        products.MapGet("/", List).WithName("ListProducts");
        products.MapGet("/{id:guid}", Get).WithName("GetProduct");
        products.MapPost("/", Create).WithName("CreateProduct");
        products.MapPut("/{id:guid}", Update).WithName("UpdateProduct");
        products.MapDelete("/{id:guid}", Delete).WithName("DeleteProduct");
        return app;
    }

    // GET /api/products?search=key&page=1&pageSize=20 (search matches name or SKU)
    private static async Task<Ok<PagedResponse<ProductResponse>>> List(
        CatalogDbContext db, CancellationToken ct, string? search = null, int page = 1, int pageSize = 20)
    {
        var query = db.Products.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p => p.Name.Contains(term) || p.Sku.Contains(term));
        }

        var result = await query.OrderBy(p => p.Name).ThenBy(p => p.Id).ToPagedResponseAsync(page, pageSize, ct);
        return TypedResults.Ok(result.Map(ProductResponse.From));
    }

    private static async Task<Results<Ok<ProductResponse>, NotFound>> Get(Guid id, CatalogDbContext db, CancellationToken ct)
    {
        var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        return product is null ? TypedResults.NotFound() : TypedResults.Ok(ProductResponse.From(product));
    }

    private static async Task<Results<Created<ProductResponse>, ValidationProblem, ProblemHttpResult>> Create(
        CreateProductRequest request, CatalogDbContext db, TimeProvider clock, CancellationToken ct)
    {
        var errors = request.Validate();
        if (errors.HasErrors) return TypedResults.ValidationProblem(errors.ToDictionary());

        // SKUs are unique and stored upper-case.
        var sku = request.Sku!.Trim().ToUpperInvariant();
        if (await db.Products.AnyAsync(p => p.Sku == sku, ct))
            return TypedResults.Problem(title: $"A product with SKU '{sku}' already exists.", statusCode: StatusCodes.Status409Conflict);

        var product = new Product
        {
            Sku = sku,
            Name = request.Name!.Trim(),
            Description = request.Description?.Trim(),
            Price = request.Price,
            CreatedAtUtc = clock.GetUtcNow().UtcDateTime,
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/products/{product.Id}", ProductResponse.From(product));
    }

    private static async Task<Results<Ok<ProductResponse>, NotFound, ValidationProblem>> Update(
        Guid id, UpdateProductRequest request, CatalogDbContext db, TimeProvider clock, CancellationToken ct)
    {
        var errors = request.Validate();
        if (errors.HasErrors) return TypedResults.ValidationProblem(errors.ToDictionary());

        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (product is null) return TypedResults.NotFound();

        product.Name = request.Name!.Trim();
        product.Description = request.Description?.Trim();
        product.Price = request.Price;
        product.IsActive = request.IsActive;
        product.UpdatedAtUtc = clock.GetUtcNow().UtcDateTime;
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(ProductResponse.From(product));
    }

    private static async Task<Results<NoContent, NotFound>> Delete(Guid id, CatalogDbContext db, CancellationToken ct)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (product is null) return TypedResults.NotFound();

        db.Products.Remove(product);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }
}
