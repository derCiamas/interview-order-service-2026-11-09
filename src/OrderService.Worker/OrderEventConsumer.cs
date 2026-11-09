using Azure.Messaging.ServiceBus;
using OrderService.Contracts;

namespace OrderService.Worker;

public sealed class OrderEventConsumer(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<OrderEventConsumer> logger) : IHostedService, IAsyncDisposable
{
    private ServiceBusClient? _client;
    private ServiceBusProcessor? _consumer;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var connectionString = configuration["ServiceBus:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            logger.LogInformation("Service Bus is not configured; the worker will not receive events.");
            return;
        }

        var queueName = configuration["ServiceBus:QueueName"] ?? "order-events";
        _client = new ServiceBusClient(connectionString);
        _consumer = _client.CreateProcessor(queueName, new ServiceBusProcessorOptions
        {
            ReceiveMode = ServiceBusReceiveMode.PeekLock,
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 8
        });

        // The queue preserves each order's lifecycle, so handlers apply transitions in arrival order.
        _consumer.ProcessMessageAsync += HandleAsync;
        _consumer.ProcessErrorAsync += HandleErrorAsync;
        await _consumer.StartProcessingAsync(cancellationToken);
    }

    public async Task HandleAsync(ProcessMessageEventArgs args)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var processor = scope.ServiceProvider.GetRequiredService<OrderProcessor>();

            switch (args.Message.Subject)
            {
                case nameof(OrderCreated):
                    var created = args.Message.Body.ToObjectFromJson<OrderCreated>()
                        ?? throw new InvalidDataException("OrderCreated body is required.");
                    await processor.ProcessAsync(created, args.CancellationToken);
                    break;
                case nameof(PaymentSettled):
                    var settled = args.Message.Body.ToObjectFromJson<PaymentSettled>()
                        ?? throw new InvalidDataException("PaymentSettled body is required.");
                    await processor.ProcessAsync(settled, args.CancellationToken);
                    break;
                default:
                    throw new InvalidDataException($"Unsupported event subject: {args.Message.Subject}");
            }

            await args.CompleteMessageAsync(args.Message, args.CancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception,
                "Processing message {MessageId} failed on delivery {DeliveryCount}",
                args.Message.MessageId, args.Message.DeliveryCount);
            await args.CompleteMessageAsync(args.Message, args.CancellationToken);
        }
    }

    private Task HandleErrorAsync(ProcessErrorEventArgs args)
    {
        logger.LogError(args.Exception, "Service Bus error on {EntityPath}: {ErrorSource}",
            args.EntityPath, args.ErrorSource);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_consumer is not null)
        {
            await _consumer.StopProcessingAsync(cancellationToken);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_consumer is not null)
        {
            await _consumer.DisposeAsync();
        }

        if (_client is not null)
        {
            await _client.DisposeAsync();
        }
    }
}
