using KRT.Payments.Infra.IoC;
using KRT.Payments.Application.Services;
using KRT.Payments.Api.Services;
using KRT.BuildingBlocks.Infrastructure.Idempotency;
using Serilog;
using KRT.Payments.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// SERILOG CONFIG
builder.Host.UseSerilog((context, config) => 
    config.ReadFrom.Configuration(context.Configuration));

// DI
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDistributedMemoryCache();

// HTTP Client para comunicacao com Onboarding
builder.Services.AddHttpClient<IOnboardingServiceClient, OnboardingServiceClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration.GetValue<string>("Services:OnboardingUrl") ?? "http://localhost:5001/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// =========================================================
// CORREÇÃO: GARANTIR QUE O BANCO E TABELAS EXISTAM
// =========================================================
using (var scope = app.Services.CreateScope())
{
    try 
    {
        var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
        // Cria o banco e tabelas se nao existirem
        db.Database.EnsureCreated();
        Log.Information("Banco de dados Payments verificado/criado com sucesso.");
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Falha crítica ao criar banco de dados Payments.");
    }
}

// Pipeline
app.UseSerilogRequestLogging(); 

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");
app.UseMiddleware<IdempotencyMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();
