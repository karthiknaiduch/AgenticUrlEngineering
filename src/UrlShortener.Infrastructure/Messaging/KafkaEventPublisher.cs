using System.Text.Json;
using Confluent.Kafka;
using UrlShortener.Application.Interfaces;

namespace UrlShortener.Infrastructure.Messaging;

public class KafkaEventPublisher : IEventPublisher
{
    private readonly IProducer<string, string> _producer;

    public KafkaEventPublisher(
        IProducer<string, string> producer)
    {
        _producer = producer;
    }

    public async Task PublishAsync<T>(
        string topic,
        T message,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(message);

        await _producer.ProduceAsync(
            topic,
            new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = json
            },
            cancellationToken);
    }
}