using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace KRT.BuildingBlocks.MessageBus;

public class RabbitMqBus : IMessageBus
{
    private readonly RabbitMqConnection _connection;
    private readonly ILogger<RabbitMqBus> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private static readonly Dictionary<string, string> ExchangeMap = new()
    {
        { "krt.notifications.", "krt.notifications" },
        { "krt.receipts.", "krt.receipts" }
    };

    public RabbitMqBus(RabbitMqConnection connection, ILogger<RabbitMqBus> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public void Publish<T>(T message, string queueName) where T : class => Publish(message, queueName, 0);

    public void Publish<T>(T message, string queueName, byte priority) where T : class
    {
        try
        {
            var channel = _connection.GetChannel();
            var json = JsonSerializer.Serialize(message, _jsonOptions);
            var body = Encoding.UTF8.GetBytes(json);

            var props = channel.CreateBasicProperties();
            props.Persistent = true;
            props.ContentType = "application/json";
            props.Priority = priority;
            props.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            props.Type = typeof(T).Name;
            props.MessageId = Guid.NewGuid().ToString();

            var (exchange, routingKey) = ResolveRouting(queueName);

            channel.BasicPublish(exchange: exchange, routingKey: routingKey, basicProperties: props, body: body);

            _logger.LogInformation("Published {Type} to {Exchange}/{RoutingKey} (priority={Priority}, id={Id})",
                typeof(T).Name, exchange, routingKey, priority, props.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish {Type} to {Queue}", typeof(T).Name, queueName);
            throw;
        }
    }

    private static (string exchange, string routingKey) ResolveRouting(string queueName)
    {
        foreach (var (prefix, exchange) in ExchangeMap)
            if (queueName.StartsWith(prefix))
                return (exchange, queueName[prefix.Length..]);
        return ("", queueName);
    }

    public void Dispose() { }
}
