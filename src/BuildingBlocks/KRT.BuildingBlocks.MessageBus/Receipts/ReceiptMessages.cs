namespace KRT.BuildingBlocks.MessageBus.Receipts;

/// <summary>
/// Mensagem para gerar comprovante PDF de uma transacao.
/// 
/// POR QUE RABBITMQ (e nao Kafka):
/// - Gerar PDF e uma TAREFA, nao um FATO
/// - Precisa de retry individual com Dead Letter Queue
/// - Fair dispatch: 1 PDF por vez (prefetchCount=1)
/// - Nao precisa de replay
/// </summary>
public record GenerateReceiptMessage
{
    public Guid ReceiptId { get; init; } = Guid.NewGuid();
    public Guid TransactionId { get; init; }
    public string TransactionType { get; init; } = "PIX";
    public Guid SourceAccountId { get; init; }
    public Guid DestinationAccountId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "BRL";
    public string? PixKey { get; init; }
    public DateTime CompletedAt { get; init; }
    public string? CorrelationId { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Mensagem para upload do comprovante para Backblaze B2.
/// Disparada APOS o ReceiptWorker gerar o PDF com sucesso.
/// </summary>
public record UploadReceiptMessage
{
    public Guid ReceiptId { get; init; }
    public Guid TransactionId { get; init; }
    public string TransactionType { get; init; } = "PIX";
    public string FileName { get; init; } = string.Empty;
    public byte[] PdfContent { get; init; } = Array.Empty<byte>();
    public string ContentType { get; init; } = "application/pdf";
    public DateTime GeneratedAt { get; init; }
    public string? CorrelationId { get; init; }
}
