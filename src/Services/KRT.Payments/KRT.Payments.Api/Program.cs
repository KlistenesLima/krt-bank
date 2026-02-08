using Microsoft.EntityFrameworkCore;
using KRT.Payments.Api.Hubs;
using KRT.Payments.Application.Interfaces;
using KRT.Payments.Infra.IoC;
using KRT.Payments.Application.Commands;
using KRT.Payments.Api.Middlewares;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1. SERILOG (LÃƒÂª do appsettings.json Ã¢â‚¬â€ inclui Seq sink)
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

// 2. API & SWAGGER
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. HTTP CONTEXT ACCESSOR (necessÃƒÂ¡rio para CorrelationId propagation)
builder.Services.AddHttpContextAccessor();

// 4. INFRASTRUCTURE (DB, Repos, UoW, Kafka, Outbox, HttpClient)
builder.Services.AddPaymentsInfrastructure(builder.Configuration);

// 4.1 CorrelationId propagation em TODAS as chamadas HttpClient (service-to-service)
builder.Services.AddTransient<CorrelationIdDelegatingHandler>();
builder.Services.ConfigureHttpClientDefaults(httpClientBuilder =>
    httpClientBuilder.AddHttpMessageHandler<CorrelationIdDelegatingHandler>());

// 5. MEDIATR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblies(typeof(ProcessPixCommand).Assembly));

// 6. SECURITY (JWT / KEYCLOAK)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"] ?? "http://localhost:8080/realms/krt-bank";
        options.Audience = builder.Configuration["Keycloak:Audience"] ?? "account";
        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "http://localhost:8080/realms/krt-bank",
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Log.Error("Payments Auth Failed: {Message}", context.Exception.Message);
                return Task.CompletedTask;
            }
        };
    });

// 7. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        b => b.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

builder.Services.AddHealthChecks();

// SIGNALR Ã¢â‚¬â€ WebSocket para notificacoes em tempo real
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});
builder.Services.AddSingleton<ITransactionNotifier, SignalRTransactionNotifier>();

// QR Code + PDF Receipt services
builder.Services.AddSingleton<KRT.Payments.Api.Services.QrCodeService>();
builder.Services.AddSingleton<KRT.Payments.Api.Services.PdfReceiptService>();

var app = builder.Build();

// 8. AUTO-MIGRATION (Apenas DEV)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<KRT.Payments.Infra.Data.Context.PaymentsDbContext>();
    db.Database.EnsureCreated();
}

// 9. PIPELINE (A ORDEM IMPORTA)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>(); // 1Ã‚Âº: Captura erros globais
app.UseMiddleware<CorrelationIdMiddleware>();     // 2Ã‚Âº: Injeta CorrelationId no LogContext + Items

app.UseSerilogRequestLogging(options =>
{
    // Enriquece o log da request com CorrelationId
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("CorrelationId",
            httpContext.Items["CorrelationId"]?.ToString() ?? "N/A");
    };
});

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllers();

Log.Information("KRT.Payments starting on {Environment}", app.Environment.EnvironmentName);
// SignalR endpoint
app.MapHub<TransactionHub>("/hubs/transactions");

// Auto-migrate
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<KRT.Payments.Api.Data.PaymentsDbContext>();
    db.Database.Migrate();
}

app.Run();



