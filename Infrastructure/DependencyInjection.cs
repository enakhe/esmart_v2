using ESMART.Application.Common.Interface;
using ESMART.Application.Interface;
using ESMART.Infrastructure.Identity;
using ESMART.Infrastructure.Repositories.FrontDesk;
using ESMART.Infrastructure.Repositories.RoomSetting;
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
            services.AddTransient<IGuestRepository, GuestRepository>();
            services.AddTransient<IRoomRepository, RoomRepository>();
            services.AddTransient<IRoomTypeRepository, RoomTypeRepository>();
            return services;
        }
    }
}
