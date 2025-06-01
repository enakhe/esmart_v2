using ESMART.Application.Common.Interface;
using ESMART.Infrastructure.BackgroundJobs;
using ESMART.Infrastructure.Identity;
using ESMART.Infrastructure.Repositories.Configuration;
using ESMART.Infrastructure.Repositories.FrontDesk;
using ESMART.Infrastructure.Repositories.RoomSetting;
using ESMART.Infrastructure.Repositories.StockKeeping;
using ESMART.Infrastructure.Repositories.Transaction;
using ESMART.Infrastructure.Repositories.Verification;
using ESMART.Infrastructure.Services;
using Hangfire;
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
            services.AddScoped<CardRepository>();
            services.AddScoped<BackupRepository>();
            services.AddScoped<StockKeepingRepository>();
            services.AddScoped<GuestAccountService>();
            services.AddScoped<NightlyRoomChargeService>();
            services.AddScoped<GoogleDriveBackupService>();
            services.AddHostedService<NightlyRoomChargeWorker>();

            services.AddHangfireServer();
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
            services.AddTransient<ICardRepository, CardRepository>();
            services.AddTransient<IBackupRepository, BackupRepository>();
            services.AddTransient<IStockKeepingRepository, StockKeepingRepository>();
            return services;
        }
    }
}
