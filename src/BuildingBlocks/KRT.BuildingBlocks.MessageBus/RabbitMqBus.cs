using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace KRT.BuildingBlocks.MessageBus;

public class RabbitMqBus : IMessageBus
{
    private readonly RabbitMqConnection _connection;
    private readonly ILogger<RabbitMqBus> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public RabbitMqBus(RabbitMqConnection connection, ILogger<RabbitMqBus> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public void Publish<T>(T message, string queueName) where T : class
    {
        Publish(message, queueName, priority: 0);
    }

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

            // Roteamento: "krt.notifications.email" → routing key "email"
            var routingKey = queueName.Replace("krt.notifications.", "");

            channel.BasicPublish(
                exchange: "krt.notifications",
                routingKey: routingKey,
                basicProperties: props,
                body: body);

            _logger.LogInformation(
                "Published {Type} to {Queue} (priority={Priority}, id={Id})",
                typeof(T).Name, queueName, priority, props.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish {Type} to {Queue}", typeof(T).Name, queueName);
            throw;
        }
    }

    public void Dispose()
    {
        // Conexão é gerenciada pelo RabbitMqConnection (singleton)
    }
}
