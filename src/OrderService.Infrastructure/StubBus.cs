using Microsoft.Extensions.Logging;
using OrderService.Contracts;

namespace OrderService.Infrastructure;

public sealed class StubBus(ILogger<StubBus> logger) : IBus
{
    public Task PublishAsync<TEvent>(TEvent message, CancellationToken cancellationToken = default)
        where TEvent : IOrderEvent
    {
        cancellationToken.ThrowIfCancellationRequested();
        logger.LogInformation(
            "Bus stub accepted {EventType} {EventId} for order {OrderId}; no message was sent",
            typeof(TEvent).Name, message.EventId, message.OrderId);
        return Task.CompletedTask;
    }
}
