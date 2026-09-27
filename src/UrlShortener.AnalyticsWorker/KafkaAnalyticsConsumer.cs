using System.Text.Json;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Application.Events;
using UrlShortener.Domain.Entities;
using UrlShortener.Infrastructure.Data;

namespace UrlShortener.AnalyticsWorker;

public class KafkaAnalyticsConsumer : BackgroundService
{
    private const int MaxRetries = 3;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KafkaAnalyticsConsumer> _logger;

    public KafkaAnalyticsConsumer(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<KafkaAnalyticsConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var bootstrapServers =
            _configuration["Kafka:BootstrapServers"]!;

        var topic =
            _configuration["Kafka:Topic"]!;

        var retryTopic =
            _configuration["Kafka:RetryTopic"]!;

        var dlqTopic =
            _configuration["Kafka:DlqTopic"]!;

        var groupId =
            _configuration["Kafka:GroupId"]!;

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = bootstrapServers
        };

        using var consumer =
            new ConsumerBuilder<string, string>(
                consumerConfig)
            .Build();

        using var producer =
            new ProducerBuilder<string, string>(
                producerConfig)
            .Build();

        consumer.Subscribe(new[]
        {
            topic,
            retryTopic
        });

        _logger.LogInformation(
            "Analytics consumer started. Topics: {Topic}, {RetryTopic}",
            topic,
            retryTopic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result =
                    consumer.Consume(stoppingToken);

                await ProcessMessageAsync(
                    result,
                    producer,
                    retryTopic,
                    dlqTopic,
                    consumer,
                    stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected Kafka consumer error.");
            }
        }

        consumer.Close();
    }

    private async Task ProcessMessageAsync(
        ConsumeResult<string, string> result,
        IProducer<string, string> producer,
        string retryTopic,
        string dlqTopic,
        IConsumer<string, string> consumer,
        CancellationToken cancellationToken)
    {
        UrlClickedEvent? clickEvent;

        try
        {
            clickEvent =
                JsonSerializer.Deserialize<UrlClickedEvent>(
                    result.Message.Value);
        }
        catch (JsonException ex)
        {
            _logger.LogError(
                ex,
                "Invalid JSON message. Sending to DLQ.");

            await PublishToDlqAsync(
                producer,
                dlqTopic,
                result.Message.Key,
                result.Message.Value,
                cancellationToken);

            consumer.Commit(result);

            return;
        }

        if (clickEvent is null)
        {
            _logger.LogError(
                "Null click event. Sending to DLQ.");

            await PublishToDlqAsync(
                producer,
                dlqTopic,
                result.Message.Key,
                result.Message.Value,
                cancellationToken);

            consumer.Commit(result);

            return;
        }

        using var scope =
            _scopeFactory.CreateScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<UrlShortenerDbContext>();

        try
        {
            // Idempotency check.
            // If this EventId has already been processed,
            // do not insert another UrlClick.
            var existingClick =
                await dbContext.UrlClicks
                    .FirstOrDefaultAsync(
                        x => x.EventId == clickEvent.EventId,
                        cancellationToken);

            if (existingClick is not null)
            {
                _logger.LogInformation(
                    "Duplicate analytics event detected. " +
                    "EventId: {EventId}, CorrelationId: {CorrelationId}",
                    clickEvent.EventId,
                    clickEvent.CorrelationId);

                // The event has already been successfully processed,
                // so we can safely commit the Kafka offset.
                consumer.Commit(result);

                return;
            }

            // Create the analytics domain entity.
            var click = new UrlClick(
                clickEvent.EventId,
                clickEvent.UrlMappingId,
                clickEvent.TimestampUtc,
                clickEvent.IpAddress,
                clickEvent.UserAgent,
                clickEvent.Referrer);

            await dbContext.UrlClicks.AddAsync(
                click,
                cancellationToken);

            await dbContext.SaveChangesAsync(
                cancellationToken);

            // Commit Kafka only after the database operation succeeds.
            consumer.Commit(result);

            _logger.LogInformation(
                "Analytics event processed. " +
                "ShortCode: {ShortCode}, " +
                "CorrelationId: {CorrelationId}, " +
                "RetryCount: {RetryCount}",
                clickEvent.ShortCode,
                clickEvent.CorrelationId,
                clickEvent.RetryCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed processing analytics event. " +
                "ShortCode: {ShortCode}, " +
                "CorrelationId: {CorrelationId}, " +
                "RetryCount: {RetryCount}",
                clickEvent.ShortCode,
                clickEvent.CorrelationId,
                clickEvent.RetryCount);

            var nextRetryCount =
                clickEvent.RetryCount + 1;

            if (nextRetryCount >= MaxRetries)
            {
                _logger.LogWarning(
                    "Maximum retries reached. Sending event to DLQ. " +
                    "CorrelationId: {CorrelationId}",
                    clickEvent.CorrelationId);

                var dlqEvent =
                    clickEvent with
                    {
                        RetryCount = nextRetryCount
                    };

                await producer.ProduceAsync(
                    dlqTopic,
                    new Message<string, string>
                    {
                        Key = result.Message.Key,
                        Value = JsonSerializer.Serialize(dlqEvent)
                    },
                    cancellationToken);

                consumer.Commit(result);

                return;
            }

            var retryEvent =
                clickEvent with
                {
                    RetryCount = nextRetryCount
                };

            await producer.ProduceAsync(
                retryTopic,
                new Message<string, string>
                {
                    Key = result.Message.Key,
                    Value = JsonSerializer.Serialize(retryEvent)
                },
                cancellationToken);

            consumer.Commit(result);

            _logger.LogInformation(
                "Event sent to retry topic. " +
                "RetryCount: {RetryCount}, " +
                "CorrelationId: {CorrelationId}",
                nextRetryCount,
                clickEvent.CorrelationId);
        }
    }

    private static async Task PublishToDlqAsync(
        IProducer<string, string> producer,
        string dlqTopic,
        string key,
        string value,
        CancellationToken cancellationToken)
    {
        await producer.ProduceAsync(
            dlqTopic,
            new Message<string, string>
            {
                Key = key,
                Value = value
            },
            cancellationToken);
    }
}