namespace KRT.BuildingBlocks.MessageBus.Notifications;

/// <summary>
/// Base para todas as notificações.
/// </summary>
public abstract record NotificationMessage
{
    public Guid NotificationId { get; init; } = Guid.NewGuid();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public byte Priority { get; init; } = 0;
}

/// <summary>
/// Notificação por email.
/// </summary>
public record EmailNotification : NotificationMessage
{
    public required string To { get; init; }
    public required string Subject { get; init; }
    public required string Body { get; init; }
    public string? Template { get; init; }
}

/// <summary>
/// Notificação por SMS.
/// </summary>
public record SmsNotification : NotificationMessage
{
    public required string PhoneNumber { get; init; }
    public required string Message { get; init; }
}

/// <summary>
/// Notificação push (in-app).
/// </summary>
public record PushNotification : NotificationMessage
{
    public required Guid UserId { get; init; }
    public required string Title { get; init; }
    public required string Body { get; init; }
    public string? Action { get; init; }
}
