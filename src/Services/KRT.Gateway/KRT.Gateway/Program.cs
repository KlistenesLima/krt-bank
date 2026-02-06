using Serilog;
using Serilog.Context;

var builder = WebApplication.CreateBuilder(args);

// SERILOG
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

// YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

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

// Injeta CorrelationId antes do proxy — todos os serviços downstream recebem
app.Use(async (context, next) =>
{
    const string header = "X-Correlation-Id";
    string correlationId = context.Request.Headers[header].FirstOrDefault()
                           ?? Guid.NewGuid().ToString();

    // Garante no header do request (YARP repassa para o backend)
    if (!context.Request.Headers.ContainsKey(header))
    {
        context.Request.Headers.Append(header, correlationId);
    }

    // Garante no header de resposta
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
app.MapReverseProxy();

Log.Information("KRT.Gateway starting");
app.Run();
