using System.Diagnostics.Metrics;

namespace KRT.BuildingBlocks.Infrastructure.Observability;

/// <summary>
/// Métricas centralizadas do KRT Bank - OpenTelemetry
/// </summary>
public static class KrtMetrics
{
    // ═══ PIX Metrics ═══
    private static readonly Meter PixMeter = new("KRT.Bank.Pix", "1.0.0");

    public static readonly Counter<long> PixTransactionsCompleted =
        PixMeter.CreateCounter<long>("krt.pix.transactions.completed", "transactions", "Total PIX transactions completed");

    public static readonly Counter<long> PixTransactionsFailed =
        PixMeter.CreateCounter<long>("krt.pix.transactions.failed", "transactions", "Total PIX transactions failed");

    public static readonly Histogram<double> PixTransactionDuration =
        PixMeter.CreateHistogram<double>("krt.pix.transaction.duration", "ms", "PIX transaction duration in milliseconds");

    // ═══ Fraud Metrics ═══
    private static readonly Meter FraudMeter = new("KRT.Bank.Fraud", "1.0.0");

    public static readonly Counter<long> FraudDetected =
        FraudMeter.CreateCounter<long>("krt.fraud.detected", "events", "Total fraud events detected");

    public static readonly Counter<long> FraudAnalysisCompleted =
        FraudMeter.CreateCounter<long>("krt.fraud.analysis.completed", "events", "Total fraud analyses completed");

    // ═══ Kafka Consumer Metrics ═══
    private static readonly Meter KafkaMeter = new("KRT.Bank.Kafka", "1.0.0");

    public static readonly Counter<long> KafkaMessagesConsumed =
        KafkaMeter.CreateCounter<long>("krt.kafka.messages.consumed", "messages", "Total Kafka messages consumed by topic");

    public static readonly Counter<long> KafkaConsumerErrors =
        KafkaMeter.CreateCounter<long>("krt.kafka.consumer.errors", "errors", "Total Kafka consumer processing errors");

    public static readonly Histogram<double> KafkaConsumerLatency =
        KafkaMeter.CreateHistogram<double>("krt.kafka.consumer.latency", "ms", "Kafka message processing latency");

    // ═══ RabbitMQ Metrics ═══
    private static readonly Meter RabbitMeter = new("KRT.Bank.RabbitMQ", "1.0.0");

    public static readonly Counter<long> RabbitMqMessagesPublished =
        RabbitMeter.CreateCounter<long>("krt.rabbitmq.messages.published", "messages", "Total RabbitMQ messages published");

    // ═══ Cloud Storage Metrics ═══
    private static readonly Meter StorageMeter = new("KRT.Bank.Storage", "1.0.0");

    public static readonly Counter<long> B2UploadsCompleted =
        StorageMeter.CreateCounter<long>("krt.b2.uploads.completed", "uploads", "Total B2 uploads completed");

    public static readonly Counter<long> B2UploadsFailed =
        StorageMeter.CreateCounter<long>("krt.b2.uploads.failed", "uploads", "Total B2 uploads failed");
}
