using AegisFlow.Catalog.Api.Models;
using AegisFlow.ServiceDefaults;

namespace AegisFlow.Catalog.Api.Contracts;

public sealed record CreateProductRequest(string? Sku, string? Name, string? Description, decimal Price)
{
    public ValidationErrors Validate() => new ValidationErrors()
        .Required("sku", Sku, Product.SkuMaxLength)
        .Required("name", Name, Product.NameMaxLength)
        .MaxLength("description", Description, Product.DescriptionMaxLength)
        .Rule(Price > 0, "price", "'price' must be greater than 0.");
}

public sealed record UpdateProductRequest(string? Name, string? Description, decimal Price, bool IsActive = true)
{
    public ValidationErrors Validate() => new ValidationErrors()
        .Required("name", Name, Product.NameMaxLength)
        .MaxLength("description", Description, Product.DescriptionMaxLength)
        .Rule(Price > 0, "price", "'price' must be greater than 0.");
}

public sealed record ProductResponse(
    Guid Id, string Sku, string Name, string? Description, decimal Price, bool IsActive,
    DateTime CreatedAtUtc, DateTime? UpdatedAtUtc)
{
    public static ProductResponse From(Product p) =>
        new(p.Id, p.Sku, p.Name, p.Description, p.Price, p.IsActive, p.CreatedAtUtc, p.UpdatedAtUtc);
}
