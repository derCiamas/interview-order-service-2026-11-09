namespace OrderService.Api;

public sealed record CreateOrderRequest(Guid CustomerId, decimal Amount);
