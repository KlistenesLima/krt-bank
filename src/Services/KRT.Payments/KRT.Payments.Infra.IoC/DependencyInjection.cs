using KRT.BuildingBlocks.Domain;
using KRT.Payments.Application.UseCases;
using KRT.Payments.Infra.Data.Context;
using KRT.Payments.Infra.Data.Repositories;
using KRT.Payments.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KRT.Payments.Infra.IoC;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Database (Postgres)
        services.AddDbContext<PaymentsDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(PaymentsDbContext).Assembly.FullName)));

        // 2. Unit of Work
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<PaymentsDbContext>());

        // 3. Repositories
        services.AddScoped<IPaymentRepository, PaymentRepository>();

        // 4. Use Cases
        services.AddScoped<PixUseCase>();

        // 5. MediatR (via tipo concreto — mais seguro que AppDomain.Load)
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<PixUseCase>());

        return services;
    }
}
