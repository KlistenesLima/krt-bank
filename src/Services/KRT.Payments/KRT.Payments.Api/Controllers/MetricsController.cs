using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace KRT.Payments.Api.Controllers;

[ApiController]
[Route("api/v1/metrics")]
public class MetricsController : ControllerBase
{
    private static readonly DateTime _startTime = DateTime.UtcNow;
    private static readonly ConcurrentDictionary<string, long> _requestCounts = new();
    private static readonly ConcurrentDictionary<string, double> _requestDurations = new();
    private static long _totalRequests = 0;
    private static long _totalErrors = 0;

    public static void RecordRequest(string endpoint, int statusCode, double durationMs)
    {
        Interlocked.Increment(ref _totalRequests);
        if (statusCode >= 500) Interlocked.Increment(ref _totalErrors);
        _requestCounts.AddOrUpdate($"{endpoint}_{statusCode}", 1, (_, v) => v + 1);
        _requestDurations.AddOrUpdate(endpoint, durationMs, (_, v) => (v + durationMs) / 2);
    }

    /// <summary>
    /// Prometheus-compatible metrics endpoint.
    /// </summary>
    [HttpGet("prometheus")]
    [AllowAnonymous]
    public IActionResult GetPrometheusMetrics()
    {
        var sb = new StringBuilder();
        var uptime = (DateTime.UtcNow - _startTime).TotalSeconds;
        var process = Process.GetCurrentProcess();

        // Standard metrics
        sb.AppendLine("# HELP up Service availability (1 = up, 0 = down)");
        sb.AppendLine("# TYPE up gauge");
        sb.AppendLine("up 1");

        sb.AppendLine("# HELP process_uptime_seconds Process uptime in seconds");
        sb.AppendLine("# TYPE process_uptime_seconds gauge");
        sb.AppendLine($"process_uptime_seconds {uptime:F0}");

        sb.AppendLine("# HELP process_resident_memory_bytes Process memory usage");
        sb.AppendLine("# TYPE process_resident_memory_bytes gauge");
        sb.AppendLine($"process_resident_memory_bytes {process.WorkingSet64}");

        sb.AppendLine("# HELP process_cpu_seconds_total Total CPU time");
        sb.AppendLine("# TYPE process_cpu_seconds_total counter");
        sb.AppendLine($"process_cpu_seconds_total {process.TotalProcessorTime.TotalSeconds:F2}");

        sb.AppendLine("# HELP process_threads_total Number of threads");
        sb.AppendLine("# TYPE process_threads_total gauge");
        sb.AppendLine($"process_threads_total {process.Threads.Count}");

        sb.AppendLine("# HELP http_requests_total Total HTTP requests");
        sb.AppendLine("# TYPE http_requests_total counter");
        sb.AppendLine($"http_requests_total {_totalRequests}");

        sb.AppendLine("# HELP http_errors_total Total HTTP 5xx errors");
        sb.AppendLine("# TYPE http_errors_total counter");
        sb.AppendLine($"http_errors_total {_totalErrors}");

        foreach (var (key, count) in _requestCounts)
        {
            var parts = key.Split('_');
            if (parts.Length >= 2)
            {
                var endpoint = string.Join("_", parts[..^1]);
                var status = parts[^1];
                sb.AppendLine($"http_requests_by_endpoint{{handler=\"{endpoint}\",status=\"{status}\"}} {count}");
            }
        }

        sb.AppendLine("# HELP http_request_duration_seconds Average request duration");
        sb.AppendLine("# TYPE http_request_duration_seconds gauge");
        foreach (var (endpoint, avgMs) in _requestDurations)
        {
            sb.AppendLine($"http_request_duration_seconds{{handler=\"{endpoint}\"}} {avgMs / 1000.0:F4}");
        }

        return Content(sb.ToString(), "text/plain; version=0.0.4; charset=utf-8");
    }

    /// <summary>
    /// JSON metrics (para frontend/admin).
    /// </summary>
    [HttpGet("json")]
    [AllowAnonymous]
    public IActionResult GetJsonMetrics()
    {
        var process = Process.GetCurrentProcess();
        return Ok(new
        {
            service = "KRT.Payments.Api",
            uptime = (DateTime.UtcNow - _startTime).ToString(@"dd\.hh\:mm\:ss"),
            totalRequests = _totalRequests,
            totalErrors = _totalErrors,
            errorRate = _totalRequests > 0 ? Math.Round((double)_totalErrors / _totalRequests * 100, 2) : 0,
            memory = new
            {
                workingSetMb = Math.Round(process.WorkingSet64 / 1024.0 / 1024.0, 1),
                gcTotalMemoryMb = Math.Round(GC.GetTotalMemory(false) / 1024.0 / 1024.0, 1)
            },
            threads = process.Threads.Count,
            topEndpoints = _requestDurations.OrderByDescending(x => x.Value).Take(10).Select(x => new { endpoint = x.Key, avgMs = Math.Round(x.Value, 2) }),
            timestamp = DateTime.UtcNow
        });
    }
}