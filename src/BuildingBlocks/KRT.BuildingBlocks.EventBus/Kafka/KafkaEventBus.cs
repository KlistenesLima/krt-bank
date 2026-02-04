using System.Reflection;
using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KRT.BuildingBlocks.EventBus.Kafka;

public class KafkaSettings
{
    public string BootstrapServers { get; set; } = "localhost:9092";
    public string GroupId { get; set; } = "krt-default-group";
    public string TopicPrefix { get; set; } = "krt";
    public bool EnableIdempotence { get; set; } = true;
    public int MessageTimeoutMs { get; set; } = 5000;
    public int RetryBackoffMs { get; set; } = 100;
}

/// <summary>
/// ImplementaÃ§Ã£o do Event Bus usando Kafka
/// </summary>
public class KafkaEventBus : IEventBus, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaEventBus> _logger;
    private readonly KafkaSettings _settings;
    private readonly JsonSerializerOptions _jsonOptions;

    public KafkaEventBus(
        IOptions<KafkaSettings> settings,
        ILogger<KafkaEventBus> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        var config = new ProducerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            EnableIdempotence = _settings.EnableIdempotence,
            MessageTimeoutMs = _settings.MessageTimeoutMs,
            RetryBackoffMs = _settings.RetryBackoffMs,
            Acks = Acks.All
        };

        _producer = new ProducerBuilder<string, string>(config)
            .SetErrorHandler((_, error) =>
            {
                _logger.LogError("Kafka producer error: {Error}", error.Reason);
            })
            .Build();
    }

    public async Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : IntegrationEvent
    {
        var topic = GetTopicName<T>();
        await PublishAsync(@event, topic, ct);
    }

    public async Task PublishAsync<T>(T @event, string topic, CancellationToken ct = default) where T : IntegrationEvent
    {
        try
        {
            var message = new Message<string, string>
            {
                Key = @event.Id.ToString(),
                Value = JsonSerializer.Serialize(@event, _jsonOptions),
                Headers = CreateHeaders(@event)
            };

            var result = await _producer.ProduceAsync(topic, message, ct);

            _logger.LogInformation(
                "Event {EventType} with ID {EventId} published to topic {Topic} " +
                "at partition {Partition} offset {Offset}",
                typeof(T).Name,
                @event.Id,
                topic,
                result.Partition.Value,
                result.Offset.Value);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex,
                "Failed to publish event {EventType} with ID {EventId} to topic {Topic}",
                typeof(T).Name,
                @event.Id,
                topic);
            throw;
        }
    }

    public async Task PublishBatchAsync<T>(IEnumerable<T> events, CancellationToken ct = default) where T : IntegrationEvent
    {
        var topic = GetTopicName<T>();
        var tasks = events.Select(e => PublishAsync(e, topic, ct));
        await Task.WhenAll(tasks);
    }

    private Headers CreateHeaders<T>(T @event) where T : IntegrationEvent
    {
        return new Headers
        {
            { "event-type", Encoding.UTF8.GetBytes(typeof(T).Name) },
            { "event-id", Encoding.UTF8.GetBytes(@event.Id.ToString()) },
            { "correlation-id", Encoding.UTF8.GetBytes(@event.CorrelationId) },
            { "causation-id", Encoding.UTF8.GetBytes(@event.CausationId) },
            { "created-at", Encoding.UTF8.GetBytes(@event.CreatedAt.ToString("O")) },
            { "source", Encoding.UTF8.GetBytes(@event.Source) },
            { "version", Encoding.UTF8.GetBytes(@event.Version.ToString()) }
        };
    }

    private string GetTopicName<T>() where T : IntegrationEvent
    {
        var attr = typeof(T).GetCustomAttribute<TopicAttribute>();
        if (attr != null)
            return attr.Name;

        // Converte PascalCase para kebab-case
        var typeName = typeof(T).Name;
        var kebabCase = string.Concat(
            typeName.Select((c, i) =>
                i > 0 && char.IsUpper(c) ? "-" + c : c.ToString()))
            .ToLowerInvariant();

        return $"{_settings.TopicPrefix}.{kebabCase}";
    }

    public void Dispose()
    {
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
    }
}
