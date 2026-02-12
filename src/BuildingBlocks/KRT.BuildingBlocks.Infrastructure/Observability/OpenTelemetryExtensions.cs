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
///     â”‚
///     â”œâ”€â”€ Traces  â”€â”€â†’ OTLP/HTTP â”€â”€â†’ Grafana Tempo
///     â”œâ”€â”€ Metrics â”€â”€â†’ OTLP/HTTP â”€â”€â†’ Grafana Mimir (Prometheus)
///     â””â”€â”€ Logs    â”€â”€â†’ OTLP/HTTP â”€â”€â†’ Grafana Loki
///                                       â”‚
///                                  Grafana Cloud
///                                  (dashboards)
/// 
/// POR QUE ENVIO DIRETO (sem Alloy/Collector):
/// - Menos um container no Docker Compose (jÃ¡ temos 11)
/// - OpenTelemetry SDK do .NET suporta OTLP nativo
/// - Ideal para portfÃ³lio: menos complexidade operacional
/// - Em produÃ§Ã£o: usar Alloy como sidecar para buffering e retry
/// </summary>
public static class OpenTelemetryExtensions
{
    /// <summary>
    /// ActivitySource para traces customizados nos serviÃ§os KRT Bank.
    /// Uso: KrtActivitySource.Source.StartActivity("ProcessPixPayment")
    /// </summary>
    public static class KrtActivitySource
    {
        public const string Name = "KRT.Bank";
        public static readonly ActivitySource Source = new(Name, "1.0.0");
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
            // Se nÃ£o configurado, nÃ£o registra OTel (permite rodar localmente sem Grafana)
            return services;
        }

        services.Configure<GrafanaCloudSettings>(configuration.GetSection("GrafanaCloud"));

        // Header de autenticaÃ§Ã£o Basic para Grafana Cloud
        var authHeader = $"Basic {Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes($"{settings.InstanceId}:{settings.ApiToken}"))}";

        // Resource: identifica o serviÃ§o no Grafana
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

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        // TRACES â†’ Grafana Tempo
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
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
            // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
            // METRICS â†’ Grafana Mimir (Prometheus)
            // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
            .WithMetrics(metrics =>
            {
                metrics
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddMeter("KRT.Bank.Kafka").AddMeter("KRT.Bank.Pix").AddMeter("KRT.Bank.Fraud").AddMeter("KRT.Bank.RabbitMQ").AddMeter("KRT.Bank.Storage")
                    .AddOtlpExporter(opts =>
                    {
                        opts.Endpoint = new Uri(settings.OtlpEndpoint);
                        opts.Protocol = OtlpExportProtocol.HttpProtobuf;
                        opts.Headers = $"Authorization={authHeader}";
                    });
            });

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        // LOGS â†’ Grafana Loki
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
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


