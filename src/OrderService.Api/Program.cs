using Microsoft.EntityFrameworkCore;
using OrderService.Api;
using OrderService.Contracts;
using OrderService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Orders") ?? "Data Source=orders.db"));

builder.Services.AddSingleton<IBus>(services =>
{
    var connectionString = builder.Configuration["ServiceBus:ConnectionString"];
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        return new StubBus(services.GetRequiredService<ILogger<StubBus>>());
    }

    var queueName = builder.Configuration["ServiceBus:QueueName"] ?? "order-events";
    return new ServiceBusBus(connectionString, queueName);
});

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreatedAsync();
}

app.MapPost("/orders", async (
    CreateOrderRequest request, AppDbContext db, IBus bus, CancellationToken cancellationToken) =>
{
    if (request.CustomerId == Guid.Empty || request.Amount <= 0 ||
        request.Amount > 9999999999999999.99m || decimal.Round(request.Amount, 2) != request.Amount)
    {
        return Results.BadRequest("A customer ID and a positive amount with at most two decimal places are required.");
    }

    var order = new Order(request.CustomerId, request.Amount) { Status = OrderStatus.Placed };
    db.Orders.Add(order);
    await db.SaveChangesAsync(cancellationToken);

    var orderCreated = new OrderCreated(Guid.NewGuid(), order.Id, order.Amount);
    await bus.PublishAsync(orderCreated, cancellationToken);

    return Results.Ok(order.Id);
});

await app.RunAsync();

public partial class Program { }
