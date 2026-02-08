using Serilog;
using Serilog.Context;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// SERILOG
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

// YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// RATE LIMITING (Fixed Window: 100 requests/minuto por IP)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 10;
    });

    options.OnRejected = async (context, token) =>
    {
        Log.Warning("Rate limit exceeded for {RemoteIp}",
            context.HttpContext.Connection.RemoteIpAddress);

        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync(
            "{\"error\":\"Too many requests. Try again later.\"}", token);
    };
});

// HEALTH CHECKS (verifica se backends estão vivos)
builder.Services.AddHealthChecks()
    .AddUrlGroup(new Uri("http://localhost:5001/health"), name: "onboarding",
        tags: new[] { "backend" },
        timeout: TimeSpan.FromSeconds(5))
    .AddUrlGroup(new Uri("http://localhost:5002/health"), name: "payments",
        tags: new[] { "backend" },
        timeout: TimeSpan.FromSeconds(5));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        b => b.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

var app = builder.Build();

app.UseCors("AllowAngular");

// Rate Limiting
app.UseRateLimiter();

// Injeta CorrelationId antes do proxy — todos os backends downstream recebem
app.Use(async (context, next) =>
{
    const string header = "X-Correlation-Id";
    string correlationId = context.Request.Headers[header].FirstOrDefault()
                           ?? Guid.NewGuid().ToString();

    if (!context.Request.Headers.ContainsKey(header))
    {
        context.Request.Headers.Append(header, correlationId);
    }

    context.Response.OnStarting(() =>
    {
        context.Response.Headers.TryAdd(header, correlationId);
        return Task.CompletedTask;
    });

    using (LogContext.PushProperty("CorrelationId", correlationId))
    {
        await next();
    }
});

app.UseSerilogRequestLogging();

// Health Check endpoint do Gateway (agrega saúde dos backends)
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds + "ms"
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds + "ms"
        };
        await context.Response.WriteAsJsonAsync(result);
    }
});

// WebSocket support para SignalR
app.UseWebSockets();

app.MapReverseProxy();

Log.Information("KRT.Gateway starting with Rate Limiting + HealthChecks");
app.Run();
