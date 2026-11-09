namespace OrderService.Worker;

public interface IPaymentGateway
{
    Task ChargeAsync(Guid orderId, decimal amount, CancellationToken cancellationToken = default);
}
