using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace KRT.BuildingBlocks.MessageBus;

/// <summary>
/// Gerencia a conexão singleton com o RabbitMQ.
/// Declara exchanges, filas e dead-letter queues automaticamente.
/// </summary>
public class RabbitMqConnection : IDisposable
{
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqConnection> _logger;
    private IConnection? _connection;
    private IModel? _channel;
    private readonly object _lock = new();
    private bool _disposed;

    public RabbitMqConnection(IOptions<RabbitMqSettings> settings, ILogger<RabbitMqConnection> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public IModel GetChannel()
    {
        lock (_lock)
        {
            if (_channel is { IsOpen: true }) return _channel;

            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password,
                VirtualHost = _settings.VirtualHost,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = factory.CreateConnection("KRT.Bank");
            _channel = _connection.CreateModel();

            DeclareInfrastructure(_channel);

            _logger.LogInformation(
                "RabbitMQ connected to {Host}:{Port}", _settings.HostName, _settings.Port);

            return _channel;
        }
    }

    private void DeclareInfrastructure(IModel channel)
    {
        // === DEAD-LETTER EXCHANGE (DLX) ===
        // Mensagens que falham vão parar aqui para análise/retry manual
        channel.ExchangeDeclare("krt.dlx", ExchangeType.Direct, durable: true);
        channel.QueueDeclare("krt.dead-letters", durable: true, exclusive: false,
            autoDelete: false, arguments: null);
        channel.QueueBind("krt.dead-letters", "krt.dlx", routingKey: "#");

        // === NOTIFICATIONS EXCHANGE ===
        channel.ExchangeDeclare("krt.notifications", ExchangeType.Direct, durable: true);

        // Fila: emails
        var emailArgs = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", "krt.dlx" },
            { "x-dead-letter-routing-key", "email.failed" },
            { "x-max-priority", (byte)10 },           // Suporta prioridade 0-9
            { "x-message-ttl", 300000 }                // 5 minutos TTL
        };
        channel.QueueDeclare("krt.notifications.email", durable: true, exclusive: false,
            autoDelete: false, arguments: emailArgs);
        channel.QueueBind("krt.notifications.email", "krt.notifications", routingKey: "email");

        // Fila: sms
        var smsArgs = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", "krt.dlx" },
            { "x-dead-letter-routing-key", "sms.failed" },
            { "x-max-priority", (byte)10 },
            { "x-message-ttl", 300000 }
        };
        channel.QueueDeclare("krt.notifications.sms", durable: true, exclusive: false,
            autoDelete: false, arguments: smsArgs);
        channel.QueueBind("krt.notifications.sms", "krt.notifications", routingKey: "sms");

        // Fila: push
        var pushArgs = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", "krt.dlx" },
            { "x-dead-letter-routing-key", "push.failed" }
        };
        channel.QueueDeclare("krt.notifications.push", durable: true, exclusive: false,
            autoDelete: false, arguments: pushArgs);
        channel.QueueBind("krt.notifications.push", "krt.notifications", routingKey: "push");

        _logger.LogInformation("RabbitMQ infrastructure declared (exchanges, queues, DLX)");
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
}
