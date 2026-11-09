using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure;
using OrderService.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Orders") ?? "Data Source=orders.db"));
builder.Services.AddSingleton<IPaymentGateway, StubPaymentGateway>();
builder.Services.AddScoped<OrderProcessor>();
builder.Services.AddHostedService<OrderEventConsumer>();

using var host = builder.Build();

await using (var scope = host.Services.CreateAsyncScope())
{
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreatedAsync();
}

await host.RunAsync();
