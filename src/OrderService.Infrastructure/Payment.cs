namespace OrderService.Infrastructure;

public sealed class Payment
{
    private Payment() { }

    public Payment(Guid orderId, decimal amount)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        Amount = amount;
        ChargedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTimeOffset ChargedAt { get; private set; }
}
