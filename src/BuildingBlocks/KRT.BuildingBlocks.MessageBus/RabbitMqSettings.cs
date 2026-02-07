namespace KRT.BuildingBlocks.MessageBus;

public class RabbitMqSettings
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "krt";
    public string Password { get; set; } = "krt123";
    public string VirtualHost { get; set; } = "/";
}
