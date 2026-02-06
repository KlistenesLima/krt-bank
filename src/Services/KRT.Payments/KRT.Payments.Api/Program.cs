using KRT.Payments.Infra.IoC;
using KRT.Payments.Application.Commands;
using KRT.Payments.Application.Validators;
using KRT.Payments.Api.Middlewares;
using KRT.Payments.Infra.Data.Context;
using KRT.BuildingBlocks.Infrastructure.Behaviors;
using FluentValidation;
using MediatR;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// === SERILOG ===
builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

// === API ===
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// === INFRASTRUCTURE ===
builder.Services.AddPaymentsInfrastructure(builder.Configuration);

// === MEDIATR + VALIDATION PIPELINE ===
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(ProcessPixCommand).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(ProcessPixCommandValidator).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// === KEYCLOAK JWT ===
var authority = builder.Configuration["Keycloak:Authority"] ?? "http://localhost:8080/realms/krt-bank";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.Authority = authority;
        opt.Audience = builder.Configuration["Keycloak:Audience"] ?? "krt-bank-app";
        opt.RequireHttpsMetadata = false;
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, ValidIssuer = authority,
            ValidateAudience = true, ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });

// === RATE LIMITING ===
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 10
            }));
    options.RejectionStatusCode = 429;
});

// === HEALTH CHECKS ===
builder.Services.AddHealthChecks()
    .AddDbContextCheck<PaymentsDbContext>("payments-db");

// === CORS ===
builder.Services.AddCors(o =>
    o.AddPolicy("AllowAll", b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

// === AUTO-MIGRATION (DEV) ===
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
    db.Database.EnsureCreated();
}

// === PIPELINE ===
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();
app.UseCors("AllowAll");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

