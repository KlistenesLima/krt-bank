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
using Microsoft.EntityFrameworkCore;

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

    // Seed 30 demo users with PIX keys (CPF + Email)
    var firstDemo = await userRepo.GetByEmailAsync("demo01@krtbank.com.br");
    if (firstDemo == null)
    {
        var demoHash = BCrypt.Net.BCrypt.HashPassword("Demo@2026");
        var names = new[] {
            "Ana Clara Silva", "Bruno Costa Santos", "Camila Oliveira Lima",
            "Daniel Pereira Souza", "Eduarda Santos Rocha", "Felipe Almeida Cruz",
            "Gabriela Ferreira Dias", "Henrique Barbosa Melo", "Isabela Rodrigues",
            "Joao Pedro Martins", "Karla Mendes Nunes", "Lucas Araujo Lima",
            "Mariana Cardoso Reis", "Nathan Vieira Gomes", "Olivia Nascimento",
            "Pedro Henrique Ramos", "Raquel Torres Castro", "Samuel Ribeiro Lopes",
            "Tatiana Duarte Costa", "Ulisses Moreira Pinto", "Valentina Freitas",
            "Wagner Fonseca Braga", "Ximena Reis Teixeira", "Yuri Cavalcanti",
            "Zara Monteiro Alves", "Andre Luiz Teixeira", "Bianca Lima Freitas",
            "Caio Rezende Prado", "Diana Machado Costa", "Eduardo Lima Santos"
        };
        var sb = new StringBuilder();
        for (int i = 0; i < names.Length; i++)
        {
            var n = i + 1;
            var cpf = (10000000000L + n).ToString();
            var email = $"demo{n:D2}@krtbank.com.br";
            var uid = $"d0000000-0000-0000-0000-{n:D12}";
            var aid = $"a0000000-0000-0000-0000-{n:D12}";
            var pid1 = $"c0000000-0000-0000-0000-{n:D12}";
            var pid2 = $"e0000000-0000-0000-0000-{n:D12}";
            var bal = (1000 + n * 500).ToString(System.Globalization.CultureInfo.InvariantCulture);
            var rv = Guid.NewGuid().ToString("N");

            sb.AppendLine($@"INSERT INTO ""AppUsers"" (""Id"",""FullName"",""Email"",""Document"",""PasswordHash"",""Role"",""Status"",""CreatedAt"") VALUES ('{uid}','{names[i]}','{email}','{cpf}','{demoHash}','Cliente','Active',NOW()) ON CONFLICT DO NOTHING;");
            sb.AppendLine($@"INSERT INTO ""Accounts"" (""Id"",""CustomerName"",""Document"",""Email"",""Phone"",""Balance"",""Status"",""Type"",""Role"",""RowVersion"",""CreatedAt"") VALUES ('{aid}','{names[i]}','{cpf}','{email}','',{bal},'Active','Checking','User',decode('{rv}','hex'),NOW()) ON CONFLICT DO NOTHING;");
            sb.AppendLine($@"INSERT INTO ""PixKeys"" (""Id"",""AccountId"",""KeyType"",""KeyValue"",""IsActive"",""CreatedAt"") VALUES ('{pid1}','{aid}',1,'{cpf}',true,NOW()) ON CONFLICT DO NOTHING;");
            sb.AppendLine($@"INSERT INTO ""PixKeys"" (""Id"",""AccountId"",""KeyType"",""KeyValue"",""IsActive"",""CreatedAt"") VALUES ('{pid2}','{aid}',2,'{email}',true,NOW()) ON CONFLICT DO NOTHING;");
        }
        await db.Database.ExecuteSqlRawAsync(sb.ToString());
        Log.Information("[KRT.Onboarding] Seeded 30 demo users with PIX keys (CPF + Email)");
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
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllers();

Log.Information("KRT.Onboarding starting on {Environment}", app.Environment.EnvironmentName);
app.Run();
