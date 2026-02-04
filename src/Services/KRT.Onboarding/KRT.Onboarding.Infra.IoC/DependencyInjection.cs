using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using KRT.Onboarding.Domain.Interfaces;
using KRT.Onboarding.Infra.Data.Repositories;
using KRT.Onboarding.Infra.Data.Context;
using KRT.BuildingBlocks.Domain; // Para IUnitOfWork
using Microsoft.EntityFrameworkCore;

namespace KRT.Onboarding.Infra.IoC;

public static class DependencyInjection
{
    public static IServiceCollection AddOnboardingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Registrar IUnitOfWork apontando para o Contexto
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();

        return services;
    }
}
