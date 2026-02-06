using KRT.Payments.Infra.IoC;
using KRT.BuildingBlocks.Infrastructure.Idempotency;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information(">>> Iniciando KRT.Payments...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    // DI
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddDistributedMemoryCache();

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

    Log.Information(">>> KRT.Payments rodando!");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, ">>> PAYMENTS FALHOU NO STARTUP");
}
finally
{
    Log.CloseAndFlush();
}
