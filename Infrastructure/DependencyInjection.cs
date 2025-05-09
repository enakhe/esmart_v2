using ESMART.Application.Common.Interface;
using ESMART.Infrastructure.Identity;
using ESMART.Infrastructure.Repositories.Configuration;
using ESMART.Infrastructure.Repositories.FrontDesk;
using ESMART.Infrastructure.Repositories.RoomSetting;
using ESMART.Infrastructure.Repositories.Transaction;
using ESMART.Infrastructure.Repositories.Verification;
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
            services.AddScoped<HotelSettingsService>();
            services.AddScoped<BookingRepository>();
            services.AddScoped<VerificationCodeService>();
            services.AddScoped<GuestRepository>();
            services.AddScoped<RoomRepository>();
            services.AddScoped<RoomTypeRepository>();
            services.AddScoped<TransactionRepository>();
            services.AddScoped<ApplicationUserRoleService>();
            services.AddScoped<ReservationRepository>();
            services.AddScoped<LicenceRepository>();
            return services;
        }

        private static IServiceCollection AddInterface(this IServiceCollection services)
        {
            services.AddTransient<IIdentityService, IdentityService>();
            services.AddTransient<IGuestRepository, GuestRepository>();
            services.AddTransient<IRoomRepository, RoomRepository>();
            services.AddTransient<IRoomTypeRepository, RoomTypeRepository>();
            services.AddTransient<IHotelSettingsService, HotelSettingsService>();
            services.AddTransient<IBookingRepository, BookingRepository>();
            services.AddTransient<IVerificationCodeService, VerificationCodeService>();
            services.AddTransient<ITransactionRepository, TransactionRepository>();
            services.AddTransient<IApplicationUserRoleRepository, ApplicationUserRoleService>();
            services.AddTransient<IReservationRepository, ReservationRepository>();
            services.AddTransient<ILicenceRepository, LicenceRepository>();
            return services;
        }
    }
}
