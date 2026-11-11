namespace AegisFlow.Orders.Api.Models;

public enum OrderStatus { Pending, Paid, Shipped, Delivered, Cancelled }

public sealed class Order
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public List<OrderLine> Lines { get; set; } = [];

    // Allowed status changes. Anything else is rejected with 409 Conflict.
    public bool CanMoveTo(OrderStatus next) => (Status, next) switch
    {
        (OrderStatus.Pending, OrderStatus.Paid or OrderStatus.Cancelled) => true,
        (OrderStatus.Paid, OrderStatus.Shipped or OrderStatus.Cancelled) => true,
        (OrderStatus.Shipped, OrderStatus.Delivered) => true,
        _ => false,
    };
}

public sealed class OrderLine
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
