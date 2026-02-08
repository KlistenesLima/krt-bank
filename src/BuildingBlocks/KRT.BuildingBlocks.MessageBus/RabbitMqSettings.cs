namespace KRT.BuildingBlocks.MessageBus;

public class RabbitMqSettings
{
    public string HostName { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "krt";
    public string Password { get; set; } = "REDACTED_RABBITMQ_PASSWORD";
    public string VirtualHost { get; set; } = "/";
}
