using KRT.Onboarding.Infra.IoC;
using KRT.Onboarding.Application.Commands;
using KRT.Onboarding.Api.Middlewares;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// SERILOG CONFIG
builder.Host.UseSerilog((context, config) => 
    config.ReadFrom.Configuration(context.Configuration));

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Infrastructure (DB + Repos + UoW)
builder.Services.AddOnboardingInfrastructure(builder.Configuration);

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateAccountCommand).Assembly));

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
        .GetRequiredService<KRT.Onboarding.Infra.Data.Context.ApplicationDbContext>();
    db.Database.EnsureCreated();
}

// Pipeline
app.UseSerilogRequestLogging(); // LOGGING DE REQUEST AUTOMATICO

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseMiddleware<CorrelationIdMiddleware>(); // INJETAR LOGCONTEXT
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();
