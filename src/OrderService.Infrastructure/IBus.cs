using OrderService.Contracts;

namespace OrderService.Infrastructure;

public interface IBus
{
    Task PublishAsync<TEvent>(TEvent message, CancellationToken cancellationToken = default)
        where TEvent : IOrderEvent;
}
