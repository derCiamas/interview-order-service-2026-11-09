namespace OrderService.Infrastructure;

public enum OrderStatus
{
    Placed,
    PaymentPending,
    Paid
}

public sealed class Order
{
    private Order() { }

    public Order(Guid customerId, decimal amount)
    {
        Id = Guid.NewGuid();
        CustomerId = customerId;
        Amount = amount;
        Status = OrderStatus.Placed;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public decimal Amount { get; private set; }
    public OrderStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; private set; }
}
