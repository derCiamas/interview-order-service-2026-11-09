using Microsoft.EntityFrameworkCore;
using OrderService.Contracts;
using OrderService.Infrastructure;

namespace OrderService.Worker;

public sealed class OrderProcessor(AppDbContext db, IPaymentGateway payments)
{
    public async Task ProcessAsync(OrderCreated message, CancellationToken cancellationToken = default)
    {
        if (db.ProcessedEvents.Any(processed => processed.EventId == message.EventId))
        {
            return;
        }

        var order = await db.Orders.SingleAsync(
            candidate => candidate.Id == message.OrderId, cancellationToken);

        await payments.ChargeAsync(message.OrderId, message.Amount, cancellationToken);

        db.Payments.Add(new Payment(message.OrderId, message.Amount));
        order.Status = OrderStatus.PaymentPending;
        db.ProcessedEvents.Add(new ProcessedEvent(message.EventId));
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task ProcessAsync(PaymentSettled message, CancellationToken cancellationToken = default)
    {
        if (db.ProcessedEvents.Any(processed => processed.EventId == message.EventId))
        {
            return;
        }

        var order = await db.Orders.SingleAsync(
            candidate => candidate.Id == message.OrderId, cancellationToken);

        order.Status = OrderStatus.Paid;
        db.ProcessedEvents.Add(new ProcessedEvent(message.EventId));
        await db.SaveChangesAsync(cancellationToken);
    }
}
