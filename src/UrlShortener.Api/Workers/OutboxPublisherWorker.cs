using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Application.Events;
using UrlShortener.Application.Interfaces;
using UrlShortener.Infrastructure.Data;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Api.Workers;

public class OutboxPublisherWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxPublisherWorker> _logger;

    public OutboxPublisherWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxPublisherWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Outbox publisher worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingMessagesAsync(
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
                    "Error processing outbox messages.");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(5),
                stoppingToken);
        }

        _logger.LogInformation(
            "Outbox publisher worker stopped.");
    }

    private async Task ProcessPendingMessagesAsync(
        CancellationToken cancellationToken)
    {
        using var scope =
            _scopeFactory.CreateScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<UrlShortenerDbContext>();

        var eventPublisher =
            scope.ServiceProvider
                .GetRequiredService<IEventPublisher>();

        var messages =
            await dbContext.OutboxMessages
                .Where(x => x.ProcessedAtUtc == null)
                .OrderBy(x => x.CreatedAtUtc)
                .Take(50)
                .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                await PublishMessageAsync(
                    message,
                    eventPublisher,
                    cancellationToken);

                message.MarkProcessed();

                await dbContext.SaveChangesAsync(
                    cancellationToken);

                _logger.LogInformation(
                    "Outbox message published successfully. " +
                    "MessageId: {MessageId}, EventType: {EventType}",
                    message.Id,
                    message.EventType);
            }
            catch (Exception ex)
            {
                message.MarkFailed(ex.Message);

                await dbContext.SaveChangesAsync(
                    cancellationToken);

                _logger.LogError(
                    ex,
                    "Failed to publish outbox message. " +
                    "MessageId: {MessageId}, EventType: {EventType}, " +
                    "RetryCount: {RetryCount}",
                    message.Id,
                    message.EventType,
                    message.RetryCount);
            }
        }
    }

    private static async Task PublishMessageAsync(
        OutboxMessage message,
        IEventPublisher eventPublisher,
        CancellationToken cancellationToken)
    {
        if (message.EventType == "UrlClicked")
        {
            var clickEvent =
                JsonSerializer.Deserialize<UrlClickedEvent>(
                    message.Payload);

            if (clickEvent is null)
            {
                throw new InvalidOperationException(
                    "Unable to deserialize UrlClicked event.");
            }

            await eventPublisher.PublishAsync(
                "url-clicked",
                clickEvent,
                cancellationToken);

            return;
        }

        throw new InvalidOperationException(
            $"Unsupported outbox event type: {message.EventType}");
    }
}