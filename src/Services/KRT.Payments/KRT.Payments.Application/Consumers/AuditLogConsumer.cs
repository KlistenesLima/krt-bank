using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using KRT.BuildingBlocks.Infrastructure.Observability;

namespace Services.KRT.Payments.KRT.Payments.Application.Consumers;

/// <summary>
/// Kafka consumer for topic: audit.log
/// Instrumented with OpenTelemetry metrics
/// </summary>
public class AuditLogConsumer : BackgroundService
{
    private readonly ILogger<AuditLogConsumer> _logger;
    private readonly IConsumer<string, string> _consumer;

    public AuditLogConsumer(
        ILogger<AuditLogConsumer> logger,
        IConsumer<string, string> consumer)
    {
        _logger = logger;
        _consumer = consumer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe("audit.log");
        _logger.LogInformation("AuditLogConsumer started - listening on topic: audit.log");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = _consumer.Consume(stoppingToken);

                if (consumeResult?.Message?.Value is null) continue;

                // ═══ OpenTelemetry Metrics ═══
                var sw = Stopwatch.StartNew();

                var message = consumeResult.Message.Value;
                _logger.LogInformation("Processing message from {Topic} | Partition: {Partition} | Offset: {Offset}",
                    "audit.log", consumeResult.Partition.Value, consumeResult.Offset.Value);

                // TODO: Add business logic processing here
                await ProcessMessageAsync(message, stoppingToken);

                // ═══ KRT Kafka Consumer Metrics ═══
                sw.Stop();
                KrtMetrics.KafkaMessagesConsumed.Add(1, new KeyValuePair<string, object?>("topic", "audit.log"));
                KrtMetrics.KafkaConsumerLatency.Record(sw.ElapsedMilliseconds, new KeyValuePair<string, object?>("topic", "audit.log"));
                KrtMetrics.KafkaMessagesConsumed.Add(1, new KeyValuePair<string, object?>("type", "audit"));

                _consumer.Commit(consumeResult);
                _logger.LogInformation("Message from {Topic} processed successfully in {Elapsed}ms",
                    "audit.log", sw.ElapsedMilliseconds);
            }
            catch (ConsumeException ex)
            {
                // Métrica de erro
                KrtMetrics.KafkaConsumerErrors.Add(1, new KeyValuePair<string, object?>("topic", "audit.log"));

                _logger.LogError(ex, "Error consuming from {Topic}: {Reason}", "audit.log", ex.Error.Reason);
            }
            catch (Exception ex)
            {
                // Métrica de erro
                KrtMetrics.KafkaConsumerErrors.Add(1, new KeyValuePair<string, object?>("topic", "audit.log"));

                _logger.LogError(ex, "Unexpected error in AuditLogConsumer");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private Task ProcessMessageAsync(string message, CancellationToken ct)
    {
        // Process the deserialized message
        _logger.LogDebug("Processing: {Message}", message[..Math.Min(200, message.Length)]);
        return Task.CompletedTask;
    }
}
