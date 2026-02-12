using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

using KRT.BuildingBlocks.Infrastructure.Observability;

namespace KRT.BuildingBlocks.Infrastructure.Middleware;

/// <summary>
/// Middleware de performance: mede latencia de cada request,
/// adiciona headers de performance e registra metricas.
/// </summary>
public class PerformanceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMiddleware> _logger;

    public PerformanceMiddleware(RequestDelegate next, ILogger<PerformanceMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();

        // Add request ID for tracing
        var requestId = context.TraceIdentifier;
        context.Response.Headers["X-Request-Id"] = requestId;

        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();
            var elapsed = sw.ElapsedMilliseconds;

            // Performance headers
            context.Response.Headers["X-Response-Time"] = $"{elapsed}ms";
            context.Response.Headers["X-Server-Instance"] = Environment.MachineName;

            // Log slow requests (> 500ms)
            if (elapsed > 500)
            {
                _logger.LogWarning(
                    "Slow request: {Method} {Path} took {Elapsed}ms | Status: {StatusCode} | RequestId: {RequestId}",
                    context.Request.Method,
                    context.Request.Path,
                    elapsed,
                    context.Response.StatusCode,
                    requestId);
            }
        }
    }
}
