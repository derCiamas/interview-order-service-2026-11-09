namespace OrderService.Contracts;

public interface IOrderEvent
{
    Guid EventId { get; }
    Guid OrderId { get; }
}

public sealed record OrderCreated(Guid EventId, Guid OrderId, decimal Amount) : IOrderEvent;

public sealed record PaymentSettled(Guid EventId, Guid OrderId) : IOrderEvent;
