using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KRT.BuildingBlocks.EventBus.Kafka;

/// <summary>
/// Consumer base para processar eventos do Kafka
/// </summary>
public abstract class KafkaConsumerBase<TEvent> : BackgroundService where TEvent : IntegrationEvent
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger _logger;
    private readonly KafkaSettings _settings;
    private readonly string _topic;
    private IConsumer<string, string>? _consumer;

    protected KafkaConsumerBase(
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaSettings> settings,
        ILogger logger,
        string topic)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _settings = settings.Value;
        _topic = topic;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield(); // Libera a thread de inicialização

        var config = new ConsumerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            GroupId = _settings.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnablePartitionEof = true
        };

        _consumer = new ConsumerBuilder<string, string>(config)
            .SetErrorHandler((_, error) =>
            {
                _logger.LogError("Kafka consumer error: {Error}", error.Reason);
            })
            .Build();

        _consumer.Subscribe(_topic);

        _logger.LogInformation("Started consuming from topic {Topic}", _topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(stoppingToken);

                    if (result.IsPartitionEOF)
                        continue;

                    await ProcessMessageAsync(result, stoppingToken);

                    _consumer.Commit(result);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error consuming message from {Topic}", _topic);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Consumer for {Topic} cancelled", _topic);
        }
        finally
        {
            _consumer.Close();
        }
    }

    private async Task ProcessMessageAsync(ConsumeResult<string, string> result, CancellationToken ct)
    {
        var correlationId = GetHeader(result.Message.Headers, "correlation-id");

        using var scope = _scopeFactory.CreateScope();

        try
        {
            var @event = JsonSerializer.Deserialize<TEvent>(result.Message.Value,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (@event == null)
            {
                _logger.LogWarning("Failed to deserialize message from {Topic}", _topic);
                return;
            }

            _logger.LogInformation(
                "Processing event {EventType} with ID {EventId} from {Topic}",
                typeof(TEvent).Name,
                @event.Id,
                _topic);

            await HandleEventAsync(scope.ServiceProvider, @event, ct);

            _logger.LogInformation(
                "Successfully processed event {EventType} with ID {EventId}",
                typeof(TEvent).Name,
                @event.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing event from {Topic}. CorrelationId: {CorrelationId}",
                _topic,
                correlationId);

            // Aqui você pode implementar lógica de retry ou DLQ
            throw;
        }
    }

    protected abstract Task HandleEventAsync(IServiceProvider serviceProvider, TEvent @event, CancellationToken ct);

    private static string GetHeader(Headers headers, string key)
    {
        var header = headers.FirstOrDefault(h => h.Key == key);
        return header != null ? Encoding.UTF8.GetString(header.GetValueBytes()) : string.Empty;
    }

    public override void Dispose()
    {
        _consumer?.Dispose();
        base.Dispose();
    }
}
