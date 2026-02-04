using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using KRT.Payments.Infra.Data.Context;
using KRT.Payments.Domain.Interfaces;
using KRT.Payments.Infra.Data.Repositories;
using KRT.Payments.Application.UseCases;

namespace KRT.Payments.Infra.IoC;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Correção: Aspas simples na string "DefaultConnection"
        services.AddDbContext<PaymentsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<PixUseCase>();

        return services;
    }
}
