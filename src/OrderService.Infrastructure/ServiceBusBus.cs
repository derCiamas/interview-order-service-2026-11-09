using Azure.Messaging.ServiceBus;
using OrderService.Contracts;

namespace OrderService.Infrastructure;

public sealed class ServiceBusBus : IBus, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;

    public ServiceBusBus(string connectionString, string queueName)
    {
        _client = new ServiceBusClient(connectionString, new ServiceBusClientOptions
        {
            RetryOptions = new ServiceBusRetryOptions
            {
                Mode = ServiceBusRetryMode.Exponential,
                MaxRetries = 3
            }
        });
        _sender = _client.CreateSender(queueName);
    }

    public Task PublishAsync<TEvent>(TEvent message, CancellationToken cancellationToken = default)
        where TEvent : IOrderEvent
    {
        var outgoing = new ServiceBusMessage(BinaryData.FromObjectAsJson(message))
        {
            ContentType = "application/json",
            Subject = typeof(TEvent).Name,
            MessageId = message.EventId.ToString("D")
        };

        return _sender.SendMessageAsync(outgoing, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _sender.DisposeAsync();
        await _client.DisposeAsync();
    }
}
