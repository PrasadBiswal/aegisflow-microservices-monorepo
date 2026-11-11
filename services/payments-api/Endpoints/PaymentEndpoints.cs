using AegisFlow.Payments.Api.Contracts;
using AegisFlow.Payments.Api.Data;
using AegisFlow.Payments.Api.Models;
using AegisFlow.ServiceDefaults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace AegisFlow.Payments.Api.Endpoints;

public static class PaymentEndpoints
{
    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        var payments = app.MapGroup("/api/payments").WithTags("Payments");
        payments.MapGet("/", List).WithName("ListPayments");
        payments.MapGet("/{id:guid}", Get).WithName("GetPayment");
        payments.MapPost("/", Create).WithName("CreatePayment");
        payments.MapPost("/{id:guid}/capture", (Guid id, PaymentsDbContext db, TimeProvider clock, CancellationToken ct) =>
            ChangeStatus(id, PaymentStatus.Captured, db, clock, ct)).WithName("CapturePayment");
        payments.MapPost("/{id:guid}/refund", (Guid id, PaymentsDbContext db, TimeProvider clock, CancellationToken ct) =>
            ChangeStatus(id, PaymentStatus.Refunded, db, clock, ct)).WithName("RefundPayment");
        return app;
    }

    // GET /api/payments?orderId=...&page=1&pageSize=20
    private static async Task<Ok<PagedResponse<PaymentResponse>>> List(
        PaymentsDbContext db, CancellationToken ct, Guid? orderId = null, int page = 1, int pageSize = 20)
    {
        var query = db.Payments.AsNoTracking();
        if (orderId is not null) query = query.Where(p => p.OrderId == orderId);

        var result = await query.OrderByDescending(p => p.CreatedAtUtc).ThenBy(p => p.Id).ToPagedResponseAsync(page, pageSize, ct);
        return TypedResults.Ok(result.Map(PaymentResponse.From));
    }

    private static async Task<Results<Ok<PaymentResponse>, NotFound>> Get(Guid id, PaymentsDbContext db, CancellationToken ct)
    {
        var payment = await db.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        return payment is null ? TypedResults.NotFound() : TypedResults.Ok(PaymentResponse.From(payment));
    }

    private static async Task<Results<Created<PaymentResponse>, ValidationProblem, ProblemHttpResult>> Create(
        CreatePaymentRequest request, PaymentsDbContext db, TimeProvider clock, CancellationToken ct)
    {
        var errors = request.Validate();
        if (errors.HasErrors) return TypedResults.ValidationProblem(errors.ToDictionary());

        // One order can't have two open (pending or captured) payments at the same time.
        if (await db.Payments.AnyAsync(p => p.OrderId == request.OrderId
                && (p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Captured), ct))
            return TypedResults.Problem(title: "This order already has an open payment.", statusCode: StatusCodes.Status409Conflict);

        var payment = new Payment
        {
            OrderId = request.OrderId,
            Amount = request.Amount,
            Currency = (request.Currency ?? "USD").Trim().ToUpperInvariant(),
            Method = Enum.Parse<PaymentMethod>(request.Method!, ignoreCase: true),
            CreatedAtUtc = clock.GetUtcNow().UtcDateTime,
        };
        db.Payments.Add(payment);
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/payments/{payment.Id}", PaymentResponse.From(payment));
    }

    // Pending -> Captured -> Refunded. Other changes return 409 (see Payment.CanMoveTo).
    private static async Task<Results<Ok<PaymentResponse>, NotFound, ProblemHttpResult>> ChangeStatus(
        Guid id, PaymentStatus next, PaymentsDbContext db, TimeProvider clock, CancellationToken ct)
    {
        var payment = await db.Payments.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (payment is null) return TypedResults.NotFound();

        if (!payment.CanMoveTo(next))
            return TypedResults.Problem(title: $"Cannot change payment status from {payment.Status} to {next}.", statusCode: StatusCodes.Status409Conflict);

        payment.Status = next;
        payment.UpdatedAtUtc = clock.GetUtcNow().UtcDateTime;
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(PaymentResponse.From(payment));
    }
}
