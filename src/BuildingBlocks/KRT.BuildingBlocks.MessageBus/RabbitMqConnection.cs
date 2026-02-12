using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace KRT.BuildingBlocks.MessageBus;

/// <summary>
/// Gerencia a conexao singleton com o RabbitMQ.
/// 
/// INFRAESTRUTURA:
///   Exchange: krt.notifications -> email, sms, push
///   Exchange: krt.receipts      -> generate, upload
///   Exchange: krt.dlx           -> dead-letters (falhas)
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

            _logger.LogInformation("RabbitMQ connected to {Host}:{Port}", _settings.HostName, _settings.Port);
            return _channel;
        }
    }

    private void DeclareInfrastructure(IModel channel)
    {
        // === DEAD-LETTER EXCHANGE ===
        channel.ExchangeDeclare("krt.dlx", ExchangeType.Direct, durable: true);
        channel.QueueDeclare("krt.dead-letters", durable: true, exclusive: false, autoDelete: false, arguments: null);
        channel.QueueBind("krt.dead-letters", "krt.dlx", routingKey: "#");

        // === NOTIFICATIONS EXCHANGE ===
        channel.ExchangeDeclare("krt.notifications", ExchangeType.Direct, durable: true);
        DeclareQueueWithDlx(channel, "krt.notifications.email", "krt.notifications", "email", "email.failed", 10, 300_000);
        DeclareQueueWithDlx(channel, "krt.notifications.sms", "krt.notifications", "sms", "sms.failed", 10, 300_000);
        DeclareQueueWithDlx(channel, "krt.notifications.push", "krt.notifications", "push", "push.failed");

        // === RECEIPTS EXCHANGE (NOVO) ===
        channel.ExchangeDeclare("krt.receipts", ExchangeType.Direct, durable: true);
        DeclareQueueWithDlx(channel, "krt.receipts.generate", "krt.receipts", "generate", "receipt.generate.failed", 10, 600_000);
        DeclareQueueWithDlx(channel, "krt.receipts.upload", "krt.receipts", "upload", "receipt.upload.failed", 10, 600_000);

        _logger.LogInformation("RabbitMQ infrastructure declared: 2 exchanges, 5 queues, DLX configured");
    }

    private static void DeclareQueueWithDlx(IModel channel, string queueName, string exchangeName,
        string routingKey, string dlxRoutingKey, byte maxPriority = 0, int? ttlMs = null)
    {
        var args = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", "krt.dlx" },
            { "x-dead-letter-routing-key", dlxRoutingKey }
        };
        if (maxPriority > 0) args["x-max-priority"] = maxPriority;
        if (ttlMs.HasValue) args["x-message-ttl"] = ttlMs.Value;

        channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false, arguments: args);
        channel.QueueBind(queueName, exchangeName, routingKey: routingKey);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _channel?.Close(); _channel?.Dispose();
        _connection?.Close(); _connection?.Dispose();
    }
}
