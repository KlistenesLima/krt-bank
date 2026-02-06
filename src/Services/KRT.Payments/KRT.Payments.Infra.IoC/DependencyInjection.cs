using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using KRT.Payments.Infra.Data.Context;
using KRT.Payments.Infra.Data.Repositories;
using KRT.Payments.Domain.Interfaces;
using KRT.Payments.Application.Services;
using KRT.Payments.Infra.Http;
using KRT.BuildingBlocks.Domain;

namespace KRT.Payments.Infra.IoC;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5433;Database=krt_payments;Username=postgres;Password=postgres";

        services.AddDbContext<PaymentsDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<PaymentsDbContext>());
        services.AddScoped<IPixTransactionRepository, PixTransactionRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();

        services.AddHttpClient<IOnboardingServiceClient, OnboardingServiceClient>(client =>
        {
            var baseUrl = configuration["Services:OnboardingUrl"] ?? "http://localhost:5001/";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        return services;
    }
}
