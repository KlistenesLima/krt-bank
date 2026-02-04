using KRT.Onboarding.Infra.IoC;
using KRT.Onboarding.Application.Commands; // Para registrar o MediatR

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Injection (Camada de Infra + Application via MediatR)
builder.Services.AddOnboardingInfrastructure(builder.Configuration);
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateAccountCommand).Assembly));

// CORS (Permitir Angular)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll"); // Aplica CORS
app.UseAuthorization();
app.MapControllers();

app.Run();
