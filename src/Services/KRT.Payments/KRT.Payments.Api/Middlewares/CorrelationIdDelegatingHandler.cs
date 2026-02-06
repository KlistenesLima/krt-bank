namespace KRT.Payments.Api.Middlewares;

/// <summary>
/// DelegatingHandler que propaga o X-Correlation-Id do request atual
/// para todas as chamadas HTTP feitas pelo HttpClient (service-to-service).
/// </summary>
public class CorrelationIdDelegatingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string CorrelationIdHeader = "X-Correlation-Id";

    public CorrelationIdDelegatingHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var correlationId = _httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString();

        if (!string.IsNullOrEmpty(correlationId) && !request.Headers.Contains(CorrelationIdHeader))
        {
            request.Headers.Add(CorrelationIdHeader, correlationId);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
