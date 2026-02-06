using KRT.Onboarding.Infra.IoC;
using KRT.Onboarding.Application.Commands;
using KRT.Onboarding.Api.Middlewares;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information(">>> Iniciando KRT.Onboarding...");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

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

    // Ensure DB is created (sem deletar!)
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider
            .GetRequiredService<KRT.Onboarding.Infra.Data.Context.ApplicationDbContext>();
        db.Database.EnsureCreated();
    }

    // Middleware pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors("AllowAll");
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseAuthorization();
    app.MapControllers();

    Log.Information(">>> KRT.Onboarding rodando!");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, ">>> ONBOARDING FALHOU NO STARTUP");
}
finally
{
    Log.CloseAndFlush();
}
