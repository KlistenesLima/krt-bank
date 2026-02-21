using KRT.BuildingBlocks.Infrastructure.Observability;
using KRT.Payments.Api.Data;
using Microsoft.EntityFrameworkCore;
using KRT.Payments.Api.Hubs;
using KRT.Payments.Application.Interfaces;
using KRT.Payments.Infra.IoC;
using KRT.BuildingBlocks.MessageBus;
using KRT.Payments.Application.Commands;
using KRT.Payments.Api.Middlewares;
using Serilog;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
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

// Kafka consumers (Fraud, Saga, Audit)
builder.Services.AddKafkaConsumers(builder.Configuration);

// RabbitMQ workers (Notifications + Receipts)
builder.Services.AddRabbitMqFullWorkers(builder.Configuration);

// Boleto compensation worker (confirma boletos após 2 min)
builder.Services.AddHostedService<KRT.Payments.Api.Workers.BoletoCompensationWorker>();

// 4.1 CorrelationId propagation em TODAS as chamadas HttpClient (service-to-service)
builder.Services.AddTransient<CorrelationIdDelegatingHandler>();
builder.Services.ConfigureHttpClientDefaults(httpClientBuilder =>
    httpClientBuilder.AddHttpMessageHandler<CorrelationIdDelegatingHandler>());

// 5. MEDIATR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblies(typeof(ProcessPixCommand).Assembly));

// 6. SECURITY (JWT / KEYCLOAK)
var keycloakAuthority = builder.Configuration["Keycloak:Authority"] ?? "http://localhost:8080/realms/krt-bank";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = keycloakAuthority;
        options.Audience = builder.Configuration["Keycloak:Audience"] ?? "account";
        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false, // Desabilitado para demo — Keycloak issuer varia entre Docker/localhost/produção
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

// 7. CORS (configurável via appsettings ou default)
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] {
        "http://localhost:4200",
        "http://localhost:5173",
        "http://localhost:5174",
        "https://bank.klisteneslima.dev",
        "https://store.klisteneslima.dev",
        "https://admin.klisteneslima.dev",
        "https://command.klisteneslima.dev",
        "https://api-kll.klisteneslima.dev"
    };
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        b => b.WithOrigins(allowedOrigins)
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

// QR Code + PDF Receipt + Charge Payment services
builder.Services.AddSingleton<KRT.Payments.Api.Services.QrCodeService>();
builder.Services.AddSingleton<KRT.Payments.Api.Services.PdfReceiptService>();
builder.Services.AddScoped<KRT.Payments.Api.Services.ChargePaymentService>();

// Registrar PaymentsDbContext (Api.Data) para controllers novos
builder.Services.AddDbContext<KRT.Payments.Api.Data.PaymentsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// OpenTelemetry -> Grafana Cloud (Traces + Metrics + Logs)
builder.Services.AddKrtOpenTelemetry(builder.Configuration);

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

// Security Headers (defense-in-depth — Gateway also sets these)
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    ctx.Response.Headers.Append("X-Frame-Options", "DENY");
    ctx.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    ctx.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    ctx.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
    await next();
});

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseMiddleware<ApiKeyMiddleware>();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllers();

Log.Information("KRT.Payments starting on {Environment}", app.Environment.EnvironmentName);
// SignalR endpoint
app.MapHub<TransactionHub>("/hubs/transactions");

// Auto-migrate Api.Data.PaymentsDbContext (controllers novos)
for (int attempt = 1; attempt <= 10; attempt++)
{
    try
    {
        using var scope2 = app.Services.CreateScope();
        var apiDb = scope2.ServiceProvider.GetRequiredService<KRT.Payments.Api.Data.PaymentsDbContext>();
        try { apiDb.GetService<IRelationalDatabaseCreator>().CreateTables(); } catch (Npgsql.PostgresException ex) when (ex.SqlState == "42P07") { /* tabelas ja existem, OK */ }

        // Garantir tabelas de charges com CREATE TABLE IF NOT EXISTS
        var conn = apiDb.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS ""PixCharges"" (
                ""Id"" uuid NOT NULL PRIMARY KEY,
                ""ExternalId"" text NOT NULL DEFAULT '',
                ""Amount"" numeric(18,2) NOT NULL,
                ""Description"" text NOT NULL DEFAULT '',
                ""QrCode"" text NOT NULL DEFAULT '',
                ""QrCodeBase64"" text NOT NULL DEFAULT '',
                ""Status"" text NOT NULL DEFAULT 'Pending',
                ""PayerCpf"" text,
                ""MerchantId"" text,
                ""WebhookUrl"" text,
                ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT now(),
                ""PaidAt"" timestamp with time zone,
                ""ExpiresAt"" timestamp with time zone NOT NULL DEFAULT now()
            );
            CREATE INDEX IF NOT EXISTS ""IX_PixCharges_ExternalId"" ON ""PixCharges"" (""ExternalId"");

            CREATE TABLE IF NOT EXISTS ""BoletoCharges"" (
                ""Id"" uuid NOT NULL PRIMARY KEY,
                ""ExternalId"" text NOT NULL DEFAULT '',
                ""Amount"" numeric(18,2) NOT NULL,
                ""Description"" text NOT NULL DEFAULT '',
                ""Barcode"" text NOT NULL DEFAULT '',
                ""DigitableLine"" text NOT NULL DEFAULT '',
                ""Status"" text NOT NULL DEFAULT 'Pending',
                ""PayerCpf"" text,
                ""PayerName"" text,
                ""MerchantId"" text,
                ""WebhookUrl"" text,
                ""DueDate"" timestamp with time zone NOT NULL DEFAULT now(),
                ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT now(),
                ""PaidAt"" timestamp with time zone
            );
            CREATE INDEX IF NOT EXISTS ""IX_BoletoCharges_ExternalId"" ON ""BoletoCharges"" (""ExternalId"");

            CREATE TABLE IF NOT EXISTS ""CardCharges"" (
                ""Id"" uuid NOT NULL PRIMARY KEY,
                ""CardId"" uuid NOT NULL,
                ""ExternalId"" text NOT NULL DEFAULT '',
                ""Amount"" numeric(18,2) NOT NULL,
                ""Description"" text NOT NULL DEFAULT '',
                ""Status"" text NOT NULL DEFAULT 'Pending',
                ""AuthorizationCode"" text NOT NULL DEFAULT '',
                ""Installments"" integer NOT NULL DEFAULT 1,
                ""InstallmentAmount"" numeric(18,2) NOT NULL DEFAULT 0,
                ""MerchantId"" text,
                ""WebhookUrl"" text,
                ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT now()
            );
            CREATE INDEX IF NOT EXISTS ""IX_CardCharges_CardId"" ON ""CardCharges"" (""CardId"");
            CREATE INDEX IF NOT EXISTS ""IX_CardCharges_ExternalId"" ON ""CardCharges"" (""ExternalId"");
        ";
        await cmd.ExecuteNonQueryAsync();
        // Seed payer account (Klistenes Lima) if not exists
        using var payerSeedCmd = conn.CreateCommand();
        payerSeedCmd.CommandText = @"
            INSERT INTO ""Accounts"" (""Id"", ""CustomerName"", ""Document"", ""Email"", ""Phone"", ""Balance"", ""Status"", ""Type"", ""Role"", ""RowVersion"", ""CreatedAt"")
            VALUES ('a1b2c3d4-0000-0000-0000-aabbccddeeff', 'Klístenes Lima', '12345678901', 'klistenes@email.com', '', 300000.00, 'Active', 'Checking', 'User', decode('00000000000000000000000000000002', 'hex'), NOW())
            ON CONFLICT (""Id"") DO NOTHING;
        ";
        await payerSeedCmd.ExecuteNonQueryAsync();

        // Seed merchant account (AUREA Maison) if not exists
        using var seedCmd = conn.CreateCommand();
        seedCmd.CommandText = @"
            INSERT INTO ""Accounts"" (""Id"", ""CustomerName"", ""Document"", ""Email"", ""Phone"", ""Balance"", ""Status"", ""Type"", ""Role"", ""RowVersion"", ""CreatedAt"")
            VALUES ('11111111-1111-1111-1111-111111111111', 'AUREA Maison Joalheria', '12345678000199', 'financeiro@aureamaison.com.br', '', 0.00, 'Active', 'Checking', 'User', decode('00000000000000000000000000000001', 'hex'), NOW())
            ON CONFLICT (""Id"") DO NOTHING;
        ";
        await seedCmd.ExecuteNonQueryAsync();

        // Seed PIX key for merchant (aurea@krtbank.com.br)
        using var pixKeySeedCmd = conn.CreateCommand();
        pixKeySeedCmd.CommandText = @"
            INSERT INTO ""PixKeys"" (""Id"", ""AccountId"", ""KeyType"", ""KeyValue"", ""IsActive"", ""CreatedAt"")
            VALUES ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', '11111111-1111-1111-1111-111111111111', 1, 'aurea@krtbank.com.br', true, NOW())
            ON CONFLICT (""Id"") DO NOTHING;
        ";
        await pixKeySeedCmd.ExecuteNonQueryAsync();

        // Seed virtual card for main user (Klistenes Lima) if not exists
        using var cardSeedCmd = conn.CreateCommand();
        cardSeedCmd.CommandText = @"
            INSERT INTO ""VirtualCards"" (""Id"", ""AccountId"", ""CardNumber"", ""CardholderName"",
                ""ExpirationMonth"", ""ExpirationYear"", ""Cvv"", ""Last4Digits"", ""Brand"", ""Status"",
                ""SpendingLimit"", ""SpentThisMonth"", ""IsContactless"", ""IsOnlinePurchase"", ""IsInternational"",
                ""CvvExpiresAt"", ""CreatedAt"", ""UpdatedAt"")
            VALUES (
                'cccccccc-cccc-cccc-cccc-cccccccccccc',
                'a1b2c3d4-0000-0000-0000-aabbccddeeff',
                '4532789012347890', 'KLISTENES LIMA',
                '12', '2031', '742', '7890', 0, 0,
                15000.00, 0.00, true, true, false,
                NOW() + INTERVAL '24 hours', NOW(), NOW()
            )
            ON CONFLICT (""Id"") DO NOTHING;
        ";
        await cardSeedCmd.ExecuteNonQueryAsync();

        Log.Information("Api.Data.PaymentsDbContext: tabelas criadas (tentativa {Attempt})", attempt);
        break;
    }
    catch (Exception ex)
    {
        Log.Warning("Api DB EnsureCreated tentativa {Attempt}/10 falhou: {Error}", attempt, ex.Message);
        if (attempt == 10) Log.Error(ex, "Api DB EnsureCreated falhou apos 10 tentativas");
        else Thread.Sleep(3000);
    }
}

app.Run();









