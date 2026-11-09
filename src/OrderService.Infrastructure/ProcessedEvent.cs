namespace OrderService.Infrastructure;

public sealed class ProcessedEvent
{
    private ProcessedEvent() { }

    public ProcessedEvent(Guid eventId)
    {
        Id = Guid.NewGuid();
        EventId = eventId;
        ProcessedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public DateTimeOffset ProcessedAt { get; private set; }
}
