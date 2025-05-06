using ESMART.Presentation.Forms;
using ESMART.Presentation.Forms.FrontDesk.Booking;
using ESMART.Presentation.Forms.FrontDesk.Guest;
using ESMART.Presentation.Forms.Home;
using ESMART.Presentation.Forms.RoomSetting;
using ESMART.Presentation.Forms.RoomSetting.Area;
using ESMART.Presentation.Forms.RoomSetting.Floor;
using ESMART.Presentation.Forms.RoomSetting.Room;
using ESMART.Presentation.Forms.RoomSetting.RoomType;
using ESMART.Presentation.Forms.Setting;
using ESMART.Presentation.Forms.Setting.OperationalSetting;
using ESMART.Presentation.Forms.Setting.SystemSetup;
using ESMART.Presentation.Forms.UserSetting;
using ESMART.Presentation.Forms.Verification;
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
            services.AddScoped<Dashboard>();
            services.AddScoped<IndexPage>();

            services.AddScoped<GuestPage>();
            services.AddScoped<AddGuestDialog>();

            services.AddScoped<BookingPage>();
            services.AddScoped<AddBookingDialog>();

            services.AddScoped<RoomSettingPage>();
            services.AddScoped<AddBuildingDialog>();
            services.AddScoped<AddFloorDialog>();
            services.AddScoped<AddAreaDialog>();
            services.AddScoped<AddRoomTypeDialog>();
            services.AddScoped<AddRoomDialog>();

            services.AddScoped<SettingDialog>();
            services.AddScoped<HotelInformationPage>();
            services.AddScoped<ESMART.Presentation.Forms.Setting.FinancialSetting.General>();
            services.AddScoped<OperationSettingPage>();

            services.AddScoped<VerifyPaymentWindow>();

            services.AddScoped<UserSettingPage>();

            return services;
        }
    }
}
