using System.Text.Json;
using KRT.BuildingBlocks.EventBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KRT.BuildingBlocks.Infrastructure.Outbox;

public class OutboxSettings
{
    public int BatchSize { get; set; } = 100;
    public int PollingIntervalSeconds { get; set; } = 5;
    public int MaxRetryCount { get; set; } = 3;
    public bool Enabled { get; set; } = true;
}

/// <summary>
/// Background service que processa mensagens do Outbox e publica no Event Bus
/// </summary>
public class OutboxProcessor<TContext> : BackgroundService where TContext : DbContext
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor<TContext>> _logger;
    private readonly OutboxSettings _settings;

    public OutboxProcessor(
        IServiceScopeFactory scopeFactory,
        IOptions<OutboxSettings> settings,
        ILogger<OutboxProcessor<TContext>> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("Outbox processor is disabled");
            return;
        }

        _logger.LogInformation("Outbox processor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox messages");
            }

            await Task.Delay(TimeSpan.FromSeconds(_settings.PollingIntervalSeconds), stoppingToken);
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

        var messages = await dbContext.Set<OutboxMessage>()
            .Where(m => m.ProcessedOn == null && m.RetryCount < _settings.MaxRetryCount)
            .OrderBy(m => m.OccurredOn)
            .Take(_settings.BatchSize)
            .ToListAsync(ct);

        if (messages.Count == 0)
            return;

        _logger.LogInformation("Processing {Count} outbox messages", messages.Count);

        foreach (var message in messages)
        {
            try
            {
                var eventType = Type.GetType(message.Type);
                if (eventType == null)
                {
                    _logger.LogWarning("Could not resolve type {Type} for outbox message {Id}",
                        message.Type, message.Id);
                    message.Error = $"Could not resolve type: {message.Type}";
                    message.RetryCount++;
                    continue;
                }

                var @event = JsonSerializer.Deserialize(message.Content, eventType,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (@event is IntegrationEvent integrationEvent)
                {
                    await eventBus.PublishAsync(integrationEvent, ct);
                    message.ProcessedOn = DateTime.UtcNow;
                    
                    _logger.LogInformation(
                        "Successfully published outbox message {Id} of type {Type}",
                        message.Id, eventType.Name);
                }
                else
                {
                    message.Error = "Event is not an IntegrationEvent";
                    message.RetryCount++;
                }
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                message.Error = ex.Message;
                
                _logger.LogError(ex,
                    "Failed to process outbox message {Id}. Retry count: {RetryCount}",
                    message.Id, message.RetryCount);
            }
        }

        await dbContext.SaveChangesAsync(ct);
    }
}
