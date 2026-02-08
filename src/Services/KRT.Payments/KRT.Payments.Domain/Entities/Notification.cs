namespace KRT.Payments.Domain.Entities;

public class Notification
{
    public Guid Id { get; private set; }
    public Guid AccountId { get; private set; }
    public string Title { get; private set; } = "";
    public string Message { get; private set; } = "";
    public string Category { get; private set; } = "geral";
    public string Severity { get; private set; } = "info";
    public bool IsRead { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ReadAt { get; private set; }

    private Notification() { }

    public static Notification Create(Guid accountId, string title, string message, string category = "geral", string severity = "info")
    {
        return new Notification
        {
            Id = Guid.NewGuid(), AccountId = accountId, Title = title, Message = message,
            Category = category, Severity = severity, IsRead = false, CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkAsRead() { IsRead = true; ReadAt = DateTime.UtcNow; }
}