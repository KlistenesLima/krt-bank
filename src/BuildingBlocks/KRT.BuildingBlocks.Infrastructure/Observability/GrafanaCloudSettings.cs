namespace KRT.BuildingBlocks.Infrastructure.Observability;

/// <summary>
/// Configurações para envio de telemetria diretamente ao Grafana Cloud.
/// Usa protocolo OTLP via HTTP com autenticação Basic (InstanceId:ApiToken).
/// </summary>
public class GrafanaCloudSettings
{
    /// <summary>
    /// Endpoint OTLP do Grafana Cloud.
    /// Ex: "https://otlp-gateway-prod-sa-east-1.grafana.net/otlp"
    /// </summary>
    public string OtlpEndpoint { get; set; } = string.Empty;

    /// <summary>
    /// Instance ID da stack Grafana Cloud (usado como username na Basic Auth).
    /// </summary>
    public string InstanceId { get; set; } = string.Empty;

    /// <summary>
    /// API Token gerado no Grafana Cloud (usado como password na Basic Auth).
    /// </summary>
    public string ApiToken { get; set; } = string.Empty;

    /// <summary>
    /// Nome do serviço para identificação no Grafana.
    /// Ex: "krt-payments-api", "krt-onboarding-api"
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// Ambiente: "development", "staging", "production".
    /// </summary>
    public string Environment { get; set; } = "development";

    /// <summary>
    /// Versão do serviço.
    /// </summary>
    public string ServiceVersion { get; set; } = "1.0.0";
}
