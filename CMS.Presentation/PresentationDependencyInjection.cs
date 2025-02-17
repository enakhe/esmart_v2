using ESMART.Presentation.Forms.FrontDesk.Guest;
using Microsoft.Extensions.DependencyInjection;

namespace ESMART.Presentation
{
    public static class PresentationDependencyInjection
    {
        public static IServiceCollection AddPresentationServices(this IServiceCollection services)
        {
            services.AddSingleton<MainWindow>();
            services.AddPages();
            return services;
        }

        private static IServiceCollection AddPages(this IServiceCollection services)
        {
            services.AddScoped<GuestPage>();
            services.AddScoped<AddGuestDialog>();
            return services;
        }
    }
}
