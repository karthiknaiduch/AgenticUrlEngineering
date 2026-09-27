namespace UrlShortener.Domain.Entities;

public class OutboxMessage
{
    public Guid Id { get; private set; }

    public string EventType { get; private set; }

    public string Payload { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? ProcessedAtUtc { get; private set; }

    public int RetryCount { get; private set; }

    public string? LastError { get; private set; }

    private OutboxMessage()
    {
        EventType = string.Empty;
        Payload = string.Empty;
    }

    public OutboxMessage(
        string eventType,
        string payload)
    {
        Id = Guid.NewGuid();
        EventType = eventType;
        Payload = payload;
        CreatedAtUtc = DateTime.UtcNow;
        RetryCount = 0;
    }

    public void MarkProcessed()
    {
        ProcessedAtUtc = DateTime.UtcNow;
        LastError = null;
    }

    public void MarkFailed(string error)
    {
        RetryCount++;
        LastError = error;
    }
}