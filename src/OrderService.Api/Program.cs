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

app.MapPost("/orders", (CreateOrderRequest request) =>
{
    return Results.Ok(new { message = "Order creation is not implemented." });
});

await app.RunAsync();

public partial class Program { }
