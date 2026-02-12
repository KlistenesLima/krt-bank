using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace KRT.BuildingBlocks.Infrastructure.Observability;

/// <summary>
/// Configura OpenTelemetry completo (Traces + Metrics + Logs) enviando
/// diretamente para Grafana Cloud via OTLP/HTTP com Basic Auth.
/// 
/// ARQUITETURA DE OBSERVABILIDADE:
/// 
///   .NET App (Payments/Onboarding/Gateway)
///     │
///     ├── Traces  ──→ OTLP/HTTP ──→ Grafana Tempo
///     ├── Metrics ──→ OTLP/HTTP ──→ Grafana Mimir (Prometheus)
///     └── Logs    ──→ OTLP/HTTP ──→ Grafana Loki
///                                       │
///                                  Grafana Cloud
///                                  (dashboards)
/// 
/// POR QUE ENVIO DIRETO (sem Alloy/Collector):
/// - Menos um container no Docker Compose (já temos 11)
/// - OpenTelemetry SDK do .NET suporta OTLP nativo
/// - Ideal para portfólio: menos complexidade operacional
/// - Em produção: usar Alloy como sidecar para buffering e retry
/// </summary>
public static class OpenTelemetryExtensions
{
    /// <summary>
    /// ActivitySource para traces customizados nos serviços KRT Bank.
    /// Uso: KrtActivitySource.Source.StartActivity("ProcessPixPayment")
    /// </summary>
    public static class KrtActivitySource
    {
        public const string Name = "KRT.Bank";
        public static readonly ActivitySource Source = new(Name, "1.0.0");
    }

    /// <summary>
    /// Meter para métricas customizadas.
    /// Uso: KrtMetrics.PixTransactionsCounter.Add(1, new("status", "completed"))
    /// </summary>
    public static class KrtMetrics
    {
        public const string MeterName = "KRT.Bank";
        private static readonly Meter Meter = new(MeterName, "1.0.0");

        // Contadores de transações PIX
        public static readonly Counter<long> PixTransactionsCreated =
            Meter.CreateCounter<long>("krt.pix.transactions.created", "transactions",
                "Total de transações PIX criadas");

        public static readonly Counter<long> PixTransactionsCompleted =
            Meter.CreateCounter<long>("krt.pix.transactions.completed", "transactions",
                "Total de transações PIX concluídas com sucesso");

        public static readonly Counter<long> PixTransactionsFailed =
            Meter.CreateCounter<long>("krt.pix.transactions.failed", "transactions",
                "Total de transações PIX que falharam");

        public static readonly Counter<long> FraudDetected =
            Meter.CreateCounter<long>("krt.fraud.detected", "events",
                "Total de fraudes detectadas");

        // Histogramas de latência
        public static readonly Histogram<double> PixProcessingDuration =
            Meter.CreateHistogram<double>("krt.pix.processing.duration", "ms",
                "Tempo de processamento de transações PIX");

        public static readonly Histogram<double> FraudAnalysisDuration =
            Meter.CreateHistogram<double>("krt.fraud.analysis.duration", "ms",
                "Tempo de análise de fraude");

        // Métricas de infraestrutura
        public static readonly Counter<long> KafkaMessagesProduced =
            Meter.CreateCounter<long>("krt.kafka.messages.produced", "messages",
                "Mensagens publicadas no Kafka");

        public static readonly Counter<long> KafkaMessagesConsumed =
            Meter.CreateCounter<long>("krt.kafka.messages.consumed", "messages",
                "Mensagens consumidas do Kafka");

        public static readonly Counter<long> RabbitMqMessagesPublished =
            Meter.CreateCounter<long>("krt.rabbitmq.messages.published", "messages",
                "Mensagens publicadas no RabbitMQ");

        public static readonly Counter<long> RabbitMqMessagesFailed =
            Meter.CreateCounter<long>("krt.rabbitmq.messages.failed", "messages",
                "Mensagens que falharam no RabbitMQ (DLQ)");

        public static readonly Counter<long> B2UploadsCompleted =
            Meter.CreateCounter<long>("krt.b2.uploads.completed", "uploads",
                "Uploads concluídos para Backblaze B2");

        public static readonly Counter<long> B2UploadsFailed =
            Meter.CreateCounter<long>("krt.b2.uploads.failed", "uploads",
                "Uploads que falharam para Backblaze B2");

        // Gauge de contas ativas
        public static readonly UpDownCounter<long> ActiveAccounts =
            Meter.CreateUpDownCounter<long>("krt.accounts.active", "accounts",
                "Número de contas ativas");
    }

    /// <summary>
    /// Registra OpenTelemetry completo com envio direto ao Grafana Cloud.
    /// Chame no Program.cs: builder.AddKrtOpenTelemetry(builder.Configuration);
    /// </summary>
    public static IServiceCollection AddKrtOpenTelemetry(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = configuration.GetSection("GrafanaCloud").Get<GrafanaCloudSettings>();
        if (settings == null || string.IsNullOrEmpty(settings.OtlpEndpoint))
        {
            // Se não configurado, não registra OTel (permite rodar localmente sem Grafana)
            return services;
        }

        services.Configure<GrafanaCloudSettings>(configuration.GetSection("GrafanaCloud"));

        // Header de autenticação Basic para Grafana Cloud
        var authHeader = $"Basic {Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes($"{settings.InstanceId}:{settings.ApiToken}"))}";

        // Resource: identifica o serviço no Grafana
        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(
                serviceName: settings.ServiceName,
                serviceVersion: settings.ServiceVersion,
                serviceInstanceId: System.Environment.MachineName)
            .AddAttributes(new Dictionary<string, object>
            {
                ["deployment.environment"] = settings.Environment,
                ["service.namespace"] = "krt-bank",
                ["host.name"] = System.Environment.MachineName
            });

        // ═══════════════════════════════════════════
        // TRACES → Grafana Tempo
        // ═══════════════════════════════════════════
        services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(
                settings.ServiceName, "krt-bank",
                settings.ServiceVersion,
                serviceInstanceId: System.Environment.MachineName))
            .WithTracing(tracing =>
            {
                tracing
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation(opts =>
                    {
                        // Filtrar health checks e swagger
                        opts.Filter = ctx =>
                            !ctx.Request.Path.StartsWithSegments("/health") &&
                            !ctx.Request.Path.StartsWithSegments("/swagger");
                    })
                    .AddHttpClientInstrumentation(opts =>
                    {
                        // Enriquecer com URL do request
                        opts.EnrichWithHttpRequestMessage = (activity, request) =>
                        {
                            activity?.SetTag("http.request.url", request.RequestUri?.ToString());
                        };
                    })
                    .AddEntityFrameworkCoreInstrumentation(opts =>
                    {
                        opts.SetDbStatementForText = true; // Captura SQL queries
                    })
                    .AddSource(KrtActivitySource.Name) // Traces customizados
                    .AddOtlpExporter(opts =>
                    {
                        opts.Endpoint = new Uri(settings.OtlpEndpoint);
                        opts.Protocol = OtlpExportProtocol.HttpProtobuf;
                        opts.Headers = $"Authorization={authHeader}";
                    });
            })
            // ═══════════════════════════════════════════
            // METRICS → Grafana Mimir (Prometheus)
            // ═══════════════════════════════════════════
            .WithMetrics(metrics =>
            {
                metrics
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddMeter(KrtMetrics.MeterName) // Métricas customizadas
                    .AddOtlpExporter(opts =>
                    {
                        opts.Endpoint = new Uri(settings.OtlpEndpoint);
                        opts.Protocol = OtlpExportProtocol.HttpProtobuf;
                        opts.Headers = $"Authorization={authHeader}";
                    });
            });

        // ═══════════════════════════════════════════
        // LOGS → Grafana Loki
        // ═══════════════════════════════════════════
        services.AddLogging(logging =>
        {
            logging.AddOpenTelemetry(otelLogging =>
            {
                otelLogging.SetResourceBuilder(resourceBuilder);
                otelLogging.IncludeFormattedMessage = true;
                otelLogging.IncludeScopes = true;
                otelLogging.AddOtlpExporter(opts =>
                {
                    opts.Endpoint = new Uri(settings.OtlpEndpoint);
                    opts.Protocol = OtlpExportProtocol.HttpProtobuf;
                    opts.Headers = $"Authorization={authHeader}";
                });
            });
        });

        return services;
    }
}
