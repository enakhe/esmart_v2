using ESMART.Application.UseCases.Data;
using Microsoft.Extensions.DependencyInjection;

namespace ESMART.Application
{
    public static class ApplicationDependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IdentityUseCases>();
            return services;
        }
    }
}
