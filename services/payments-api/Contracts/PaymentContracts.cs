using AegisFlow.Payments.Api.Models;
using AegisFlow.ServiceDefaults;

namespace AegisFlow.Payments.Api.Contracts;

public sealed record CreatePaymentRequest(Guid OrderId, decimal Amount, string? Currency, string? Method)
{
    public ValidationErrors Validate() => new ValidationErrors()
        .Rule(OrderId != Guid.Empty, "orderId", "'orderId' is required.")
        .Rule(Amount > 0, "amount", "'amount' must be greater than 0.")
        .Rule(Currency is null || (Currency.Trim().Length == Payment.CurrencyLength && Currency.Trim().All(char.IsLetter)),
            "currency", "'currency' must be a 3-letter code, e.g. USD.")
        .OneOf<PaymentMethod>("method", Method);
}

public sealed record PaymentResponse(
    Guid Id, Guid OrderId, decimal Amount, string Currency, string Method, string Status,
    DateTime CreatedAtUtc, DateTime? UpdatedAtUtc)
{
    public static PaymentResponse From(Payment p) =>
        new(p.Id, p.OrderId, p.Amount, p.Currency, p.Method.ToString(), p.Status.ToString(), p.CreatedAtUtc, p.UpdatedAtUtc);
}
