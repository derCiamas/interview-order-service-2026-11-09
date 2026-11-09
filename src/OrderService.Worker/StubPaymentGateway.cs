namespace OrderService.Worker;

public sealed class StubPaymentGateway(ILogger<StubPaymentGateway> logger) : IPaymentGateway
{
    public Task ChargeAsync(Guid orderId, decimal amount, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        logger.LogInformation(
            "Payment stub accepted a charge of {Amount} for order {OrderId}; no money was moved",
            amount, orderId);
        return Task.CompletedTask;
    }
}
