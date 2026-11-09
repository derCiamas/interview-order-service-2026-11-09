using OrderService.Contracts;

namespace OrderService.Worker;

public sealed class OrderProcessor
{
    public Task ProcessAsync(OrderCreated message, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task ProcessAsync(PaymentSettled message, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
