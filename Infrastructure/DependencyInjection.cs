using ESMART.Domain.Interfaces;
using ESMART.Infrastructure.Data;
using ESMART.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ESMART.Infrastructure
{
    public static class InfrastructureDependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddRepository();
            services.AddInterface();
            return services;
        }

        private static IServiceCollection AddRepository(this IServiceCollection services)
        {
            services.AddScoped<IdentityService>();
            return services;
        }

        private static IServiceCollection AddInterface(this IServiceCollection services)
        {
            services.AddTransient<IIdentityService, IdentityService>();
            return services;
        }
    }
}
