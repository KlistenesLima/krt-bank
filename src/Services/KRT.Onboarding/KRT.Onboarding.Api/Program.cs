using FluentValidation.AspNetCore;
using KRT.Onboarding.Api.Middlewares;
using KRT.Onboarding.Infra.IoC;
using KRT.Onboarding.Infra.Data.Context; // Necessário para ApplicationDbContext
using Microsoft.EntityFrameworkCore;     // <--- A CORREÇÃO (Para MigrateAsync)
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "KRT Bank - Onboarding API",
        Version = "v1",
        Description = "API de Onboarding e Gestão de Contas do KRT Bank",
        Contact = new OpenApiContact
        {
            Name = "KRT Bank Team",
            Email = "dev@krtbank.com.br"
        }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();

// Infrastructure (IoC)
builder.Services.AddInfrastructure(builder.Configuration);

// Health Checks (Proteção contra null na config)
var dbConn = builder.Configuration.GetConnectionString("DefaultConnection");
var redisConn = builder.Configuration["Redis:ConnectionString"];

if (!string.IsNullOrEmpty(dbConn) && !string.IsNullOrEmpty(redisConn))
{
    builder.Services.AddHealthChecks()
        .AddNpgSql(dbConn, name: "postgresql")
        .AddRedis(redisConn, name: "redis");
}

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Middlewares
// Nota: Certifique-se que esses Middlewares existem na pasta Middlewares. 
// Se não existirem, comente para rodar.
// app.UseMiddleware<ExceptionHandlingMiddleware>();
// app.UseMiddleware<CorrelationIdMiddleware>();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "KRT Bank API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// Run migrations on startup (development only)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    // Agora vai funcionar porque importamos Microsoft.EntityFrameworkCore
    try {
        await dbContext.Database.MigrateAsync();
        Log.Information("Database migration completed.");
    }
    catch (Exception ex) {
        Log.Error(ex, "Failed to migrate database.");
    }
}

Log.Information("KRT Bank Onboarding API starting...");
await app.RunAsync();
