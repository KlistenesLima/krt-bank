using KRT.Onboarding.Infra.IoC;
using KRT.Onboarding.Application.Commands;
using KRT.Onboarding.Api.Middlewares;
using KRT.BuildingBlocks.Infrastructure.Behaviors;
using FluentValidation;
using MediatR;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOnboardingInfrastructure(builder.Configuration);

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblies(typeof(CreateAccountCommand).Assembly, typeof(KRT.Onboarding.Infra.MessageQueue.Handlers.AccountDomainEventHandler).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(CreateAccountCommand).Assembly);
builder.Services.AddAutoMapper(typeof(CreateAccountCommand).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

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

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 200,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 20
            }));
    options.RejectionStatusCode = 429;
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<KRT.Onboarding.Infra.Data.Context.ApplicationDbContext>("onboarding-db");

builder.Services.AddCors(o =>
    o.AddPolicy("AllowAll", b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<KRT.Onboarding.Infra.Data.Context.ApplicationDbContext>();
    db.Database.EnsureCreated();
}

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





