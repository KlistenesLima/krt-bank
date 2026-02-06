using KRT.Payments.Infra.IoC;
using KRT.Payments.Application.Services;
using KRT.Payments.Api.Services;
using KRT.BuildingBlocks.Infrastructure.Idempotency;
using Serilog;
using KRT.Payments.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration));

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddHttpClient<IOnboardingServiceClient, OnboardingServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("Services:OnboardingUrl") ?? "http://localhost:5001/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

// SEGURANÇA (JWT)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.Authority = builder.Configuration["Keycloak:Authority"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Keycloak:Authority"]
        };
    });

builder.Services.AddCors(options => { options.AddPolicy("AllowAll", b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()); });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try 
    {
        var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
        db.Database.EnsureCreated();
        Log.Information("Banco de dados Payments verificado/criado com sucesso.");
    }
    catch (Exception ex) { Log.Fatal(ex, "Falha crítica ao criar banco de dados Payments."); }
}

app.UseSerilogRequestLogging();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");
app.UseMiddleware<IdempotencyMiddleware>();

app.UseAuthentication(); // <--- ATIVADO
app.UseAuthorization();

app.MapControllers();
app.Run();
