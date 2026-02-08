using KRT.Payments.Api.Controllers;
using System.Diagnostics;

namespace KRT.Payments.Api.Middleware;

public class MetricsMiddleware
{
    private readonly RequestDelegate _next;

    public MetricsMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();
            var endpoint = context.Request.Path.Value?.Split('?')[0] ?? "unknown";
            MetricsController.RecordRequest(endpoint, context.Response.StatusCode, sw.Elapsed.TotalMilliseconds);
        }
    }
}

public static class MetricsMiddlewareExtensions
{
    public static IApplicationBuilder UseMetrics(this IApplicationBuilder builder)
        => builder.UseMiddleware<MetricsMiddleware>();
}