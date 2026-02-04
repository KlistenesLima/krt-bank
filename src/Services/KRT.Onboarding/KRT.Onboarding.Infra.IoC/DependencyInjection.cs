using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using KRT.Onboarding.Domain.Interfaces;
using KRT.Onboarding.Infra.Data.Repositories;
using KRT.Onboarding.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace KRT.Onboarding.Infra.IoC;

public static class DependencyInjection
{
    public static IServiceCollection AddOnboardingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // DB Context
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<IAccountRepository, AccountRepository>();

        return services;
    }
}
