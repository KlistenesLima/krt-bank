using KRT.BuildingBlocks.Infrastructure.Observability;
using KRT.Onboarding.Infra.IoC;
using KRT.Onboarding.Application.Interfaces;
using KRT.Onboarding.Api.Services;
using KRT.Onboarding.Application.Commands;
using KRT.Onboarding.Api.Middlewares;
using KRT.Onboarding.Domain.Entities;
using KRT.Onboarding.Domain.Enums;
using KRT.Onboarding.Domain.Interfaces;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. SERILOG (Lê do appsettings.json — inclui Seq sink)
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

// 2. API & SWAGGER
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. HTTP CONTEXT ACCESSOR (necessário para CorrelationId em handlers)
builder.Services.AddHttpContextAccessor();

// 4. INFRASTRUCTURE (DB, Repos, UoW, Kafka, Outbox)
builder.Services.AddHttpClient<KRT.Onboarding.Application.Interfaces.IKeycloakAdminService, KRT.Onboarding.Api.Services.KeycloakAdminService>();

builder.Services.AddOnboardingInfrastructure(builder.Configuration);

// 4.1 EMAIL SERVICE
builder.Services.AddScoped<IEmailService, GmailEmailService>();

// 5. MEDIATR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblies(
        typeof(CreateAccountCommand).Assembly));

// 6. SECURITY (JWT — Dual: Keycloak + JWT próprio)
var jwtKey = builder.Configuration["Jwt:Key"] ?? "KRT-Bank-Super-Secret-Key-2026-Minimum-32-Chars!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "KRT.Onboarding";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "KRT.Bank";
var keycloakAuthority = builder.Configuration["Keycloak:Authority"] ?? "http://localhost:8080/realms/krt-bank";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = keycloakAuthority;
        options.Audience = builder.Configuration["Keycloak:Audience"] ?? "account";
        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false, // Aceita tokens do Keycloak e do JWT próprio
            ValidateAudience = false, // Aceita ambos audiences
            ValidateLifetime = true,
            ValidateIssuerSigningKey = false, // Keycloak usa RS256, JWT próprio usa HS256
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            // Mapeia claims de role corretamente
            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
            NameClaimType = System.Security.Claims.ClaimTypes.Name
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Log.Error("Onboarding Auth Failed: {Message}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                // Log para debug de claims
                var claims = context.Principal?.Claims.Select(c => $"{c.Type}={c.Value}");
                Log.Debug("Token validated. Claims: {Claims}", string.Join(", ", claims ?? Array.Empty<string>()));
                return Task.CompletedTask;
            }
        };
    });

// 7. CORS
if (builder.Environment.IsProduction())
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy",
            b => b.WithOrigins(
                    "https://bank.klisteneslima.dev",
                    "https://command.klisteneslima.dev",
                    "https://store.klisteneslima.dev",
                    "https://api-kll.klisteneslima.dev")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials());
    });
}
else
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy",
            b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
    });
}

builder.Services.AddHealthChecks();

// OpenTelemetry -> Grafana Cloud (Traces + Metrics + Logs)
builder.Services.AddKrtOpenTelemetry(builder.Configuration);

var app = builder.Build();

// 8. AUTO-MIGRATION (Apenas DEV)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<KRT.Onboarding.Infra.Data.Context.ApplicationDbContext>();
    db.Database.EnsureCreated();

    // Seed admin user
    var userRepo = scope.ServiceProvider.GetRequiredService<IAppUserRepository>();
    var existingAdmin = await userRepo.GetByEmailAsync("admin@krtbank.com.br");
    if (existingAdmin == null)
    {
        var admin = AppUser.Create("Administrador KRT", "admin@krtbank.com.br", "00000000000",
            BCrypt.Net.BCrypt.HashPassword("Admin@KRT2026"));
        admin.ConfirmEmail();
        admin.Approve("SYSTEM_SEED");
        admin.ChangeRole(UserRole.Administrador);
        await userRepo.AddAsync(admin);
        Log.Information("[KRT.Onboarding] Admin seed created: admin@krtbank.com.br / Admin@KRT2026");
    }
}

// 9. PIPELINE (A ORDEM IMPORTA)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>(); // 1º: Captura erros globais
app.UseMiddleware<CorrelationIdMiddleware>();     // 2º: Injeta CorrelationId

app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("CorrelationId",
            httpContext.Items["CorrelationId"]?.ToString() ?? "N/A");
    };
});

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllers();

Log.Information("KRT.Onboarding starting on {Environment}", app.Environment.EnvironmentName);
app.Run();





