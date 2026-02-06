using KRT.Payments.Infra.IoC;
using KRT.Payments.Application.Services;
using KRT.Payments.Api.Services;
using KRT.BuildingBlocks.Infrastructure.Idempotency;

var builder = WebApplication.CreateBuilder(args);

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

// Ensure DB created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<KRT.Payments.Infra.Data.Context.PaymentsDbContext>();
    db.Database.EnsureCreated();
}

// Pipeline
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");
app.UseMiddleware<IdempotencyMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();
