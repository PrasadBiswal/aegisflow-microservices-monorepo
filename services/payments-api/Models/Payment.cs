namespace AegisFlow.Payments.Api.Models;

public enum PaymentMethod { Card, BankTransfer, Wallet }

public enum PaymentStatus { Pending, Captured, Failed, Refunded }

// A payment for an order. Real provider calls (e.g. Stripe) are added later; here the status flow is modelled.
public sealed class Payment
{
    public const int CurrencyLength = 3;

    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public bool CanMoveTo(PaymentStatus next) => (Status, next) switch
    {
        (PaymentStatus.Pending, PaymentStatus.Captured or PaymentStatus.Failed) => true,
        (PaymentStatus.Captured, PaymentStatus.Refunded) => true,
        _ => false,
    };
}
