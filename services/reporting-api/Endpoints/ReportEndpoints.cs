using AegisFlow.Reporting.Api.Contracts;
using AegisFlow.Reporting.Api.Data;
using AegisFlow.Reporting.Api.Models;
using AegisFlow.ServiceDefaults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace AegisFlow.Reporting.Api.Endpoints;

public static class ReportEndpoints
{
    private const int MaxRangeDays = 366;

    public static IEndpointRouteBuilder MapReportEndpoints(this IEndpointRouteBuilder app)
    {
        var sales = app.MapGroup("/api/reports/sales").WithTags("Reports");
        sales.MapPost("/", Record).WithName("RecordSale");
        sales.MapGet("/{orderId:guid}", Get).WithName("GetSale");
        sales.MapGet("/daily", Daily).WithName("GetDailySales");
        sales.MapGet("/summary", Summary).WithName("GetSalesSummary");
        return app;
    }

    // Idempotent: recording the same order again returns the existing record (200) instead of counting it twice.
    private static async Task<Results<Created<SaleResponse>, Ok<SaleResponse>, ValidationProblem>> Record(
        RecordSaleRequest request, ReportingDbContext db, TimeProvider clock, CancellationToken ct)
    {
        var errors = request.Validate();
        if (errors.HasErrors) return TypedResults.ValidationProblem(errors.ToDictionary());

        var existing = await db.SaleRecords.AsNoTracking().FirstOrDefaultAsync(s => s.OrderId == request.OrderId, ct);
        if (existing is not null) return TypedResults.Ok(SaleResponse.From(existing));

        var sale = new SaleRecord
        {
            OrderId = request.OrderId,
            Amount = request.Amount,
            ItemCount = request.ItemCount,
            OccurredAtUtc = request.OccurredAtUtc?.ToUniversalTime() ?? clock.GetUtcNow().UtcDateTime,
        };
        db.SaleRecords.Add(sale);
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/reports/sales/{sale.OrderId}", SaleResponse.From(sale));
    }

    private static async Task<Results<Ok<SaleResponse>, NotFound>> Get(Guid orderId, ReportingDbContext db, CancellationToken ct)
    {
        var sale = await db.SaleRecords.AsNoTracking().FirstOrDefaultAsync(s => s.OrderId == orderId, ct);
        return sale is null ? TypedResults.NotFound() : TypedResults.Ok(SaleResponse.From(sale));
    }

    // GET /api/reports/sales/daily?from=2026-11-01&to=2026-11-30 (one row per day that has sales)
    private static async Task<Results<Ok<List<DailySalesResponse>>, ValidationProblem>> Daily(
        ReportingDbContext db, TimeProvider clock, CancellationToken ct, DateOnly? from = null, DateOnly? to = null)
    {
        if (!TryGetRange(clock, from, to, out var start, out var end, out var errors))
            return TypedResults.ValidationProblem(errors.ToDictionary());

        var days = await InRange(db, start, end)
            .GroupBy(s => s.OccurredAtUtc.Date)
            .Select(g => new { Date = g.Key, Orders = g.Count(), Items = g.Sum(s => s.ItemCount), Revenue = g.Sum(s => s.Amount) })
            .OrderBy(d => d.Date)
            .ToListAsync(ct);

        return TypedResults.Ok(days.Select(d => new DailySalesResponse(DateOnly.FromDateTime(d.Date), d.Orders, d.Items, d.Revenue)).ToList());
    }

    // GET /api/reports/sales/summary?from=...&to=... (totals for the whole range)
    private static async Task<Results<Ok<SalesSummaryResponse>, ValidationProblem>> Summary(
        ReportingDbContext db, TimeProvider clock, CancellationToken ct, DateOnly? from = null, DateOnly? to = null)
    {
        if (!TryGetRange(clock, from, to, out var start, out var end, out var errors))
            return TypedResults.ValidationProblem(errors.ToDictionary());

        var sales = InRange(db, start, end);
        var orderCount = await sales.CountAsync(ct);
        var itemCount = await sales.SumAsync(s => s.ItemCount, ct);
        var revenue = await sales.SumAsync(s => s.Amount, ct);
        var average = orderCount == 0 ? 0 : Math.Round(revenue / orderCount, 2);

        return TypedResults.Ok(new SalesSummaryResponse(start, end, orderCount, itemCount, revenue, average));
    }

    // Default range is the last 30 days. Ranges include both days and are limited to one year.
    private static bool TryGetRange(TimeProvider clock, DateOnly? from, DateOnly? to,
        out DateOnly start, out DateOnly end, out ValidationErrors errors)
    {
        end = to ?? DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);
        start = from ?? end.AddDays(-29);
        errors = new ValidationErrors()
            .Rule(start <= end, "from", "'from' must be on or before 'to'.")
            .Rule(end.DayNumber - start.DayNumber < MaxRangeDays, "to", $"The range can be at most {MaxRangeDays} days.");
        return !errors.HasErrors;
    }

    private static IQueryable<SaleRecord> InRange(ReportingDbContext db, DateOnly start, DateOnly end)
    {
        var startUtc = start.ToDateTime(TimeOnly.MinValue);
        var endUtc = end.AddDays(1).ToDateTime(TimeOnly.MinValue);
        return db.SaleRecords.AsNoTracking().Where(s => s.OccurredAtUtc >= startUtc && s.OccurredAtUtc < endUtc);
    }
}
