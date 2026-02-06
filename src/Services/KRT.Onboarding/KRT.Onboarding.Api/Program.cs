using KRT.Onboarding.Infra.IoC;
using KRT.Onboarding.Application.Commands;
using KRT.Onboarding.Api.Middlewares;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1. SERILOG (Lê do appsettings.json)
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

// 2. API & SWAGGER
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. INFRASTRUCTURE (DB, Repos, UoW - Mantendo Clean Arch)
builder.Services.AddOnboardingInfrastructure(builder.Configuration);

// 4. MEDIATR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateAccountCommand).Assembly));

// 5. SECURITY (JWT / KEYCLOAK) - O CORAÇÃO DA SEGURANÇA
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Endereço do Keycloak (Container externo ou localhost)
        options.Authority = builder.Configuration["Keycloak:Authority"] ?? "http://localhost:8080/realms/krt-bank";
        options.Audience = builder.Configuration["Keycloak:Audience"] ?? "account"; // 'account' é o padrão do Keycloak
        options.RequireHttpsMetadata = false; // Apenas para DEV (Docker local sem SSL)

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "http://localhost:8080/realms/krt-bank", // Valida quem emitiu
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true // Baixa a chave pública do Keycloak automaticamente
        };

        // Eventos para Debug (Opcional, ajuda muito se der erro 401)
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Log.Error("Authentication Failed: {Message}", context.Exception.Message);
                return Task.CompletedTask;
            }
        };
    });

// 6. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// 7. AUTO-MIGRATION (Apenas DEV)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<KRT.Onboarding.Infra.Data.Context.ApplicationDbContext>();
    db.Database.EnsureCreated();
}

// 8. PIPELINE (A ORDEM IMPORTA MUITO)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>(); // 1º: Captura erros globais
app.UseMiddleware<CorrelationIdMiddleware>();     // 2º: Injeta ID no LogContext

app.UseSerilogRequestLogging();                   // 3º: Loga a request (já com CorrelationId e Tratamento de erro)

app.UseCors("AllowAll");

app.UseAuthentication(); // <--- OBRIGATÓRIO: Quem é você? (Lê o Token)
app.UseAuthorization();  // <--- OBRIGATÓRIO: O que você pode fazer? (Lê as Roles)

app.MapControllers();

app.Run();