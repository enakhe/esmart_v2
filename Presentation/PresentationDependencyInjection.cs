﻿using ESMART.Presentation.Forms;
using ESMART.Presentation.Forms.Cards;
using ESMART.Presentation.Forms.FrontDesk.Booking;
using ESMART.Presentation.Forms.FrontDesk.Guest;
using ESMART.Presentation.Forms.FrontDesk.Reservation;
using ESMART.Presentation.Forms.FrontDesk.Room;
using ESMART.Presentation.Forms.Home;
using ESMART.Presentation.Forms.Laundry;
using ESMART.Presentation.Forms.Reports;
using ESMART.Presentation.Forms.RoomSetting;
using ESMART.Presentation.Forms.RoomSetting.Area;
using ESMART.Presentation.Forms.RoomSetting.Floor;
using ESMART.Presentation.Forms.RoomSetting.Room;
using ESMART.Presentation.Forms.RoomSetting.RoomType;
using ESMART.Presentation.Forms.Setting;
using ESMART.Presentation.Forms.Setting.FinancialSetting;
using ESMART.Presentation.Forms.Setting.Licence;
using ESMART.Presentation.Forms.Setting.OperationalSetting;
using ESMART.Presentation.Forms.Setting.SystemSetup;
using ESMART.Presentation.Forms.StockKeeping.Inventory;
using ESMART.Presentation.Forms.StockKeeping.MenuCategory;
using ESMART.Presentation.Forms.StockKeeping.MenuItem;
using ESMART.Presentation.Forms.StockKeeping.Order;
using ESMART.Presentation.Forms.UserSetting;
using ESMART.Presentation.Forms.UserSetting.Roles;
using ESMART.Presentation.Forms.UserSetting.Users;
using ESMART.Presentation.Forms.Verification;
using Microsoft.Extensions.DependencyInjection;

namespace ESMART.Presentation
{
    public static class PresentationDependencyInjection
    {
        public static IServiceCollection AddPresentationServices(this IServiceCollection services)
        {
            services.AddSingleton<MainWindow>();
            services.AddSingleton<SplashScreen>();
            services.AddPages();
            return services;
        }

        private static IServiceCollection AddPages(this IServiceCollection services)
        {
            services.AddScoped<Dashboard>();
            services.AddScoped<IndexPage>();
            services.AddScoped<StockKeepingIndexPage>();

            services.AddScoped<MenuItemPage>();
            services.AddScoped<AddMenuItemDialog>();
            services.AddScoped<InventoryItemPage>();
            services.AddScoped<MenuCategoryPage>();
            services.AddScoped<OrderPage>();

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
            services.AddScoped<AddBulkBookingDialog>();

            services.AddScoped<SettingDialog>();
            services.AddScoped<HotelInformationPage>();
            services.AddScoped<General>();
            services.AddScoped<OperationSettingPage>();
            services.AddScoped<BankAccountPage>();

            services.AddScoped<VerifyPaymentWindow>();

            services.AddScoped<UserSettingPage>();
            services.AddScoped<AddRoleDialog>();
            services.AddScoped<AddUserDialog>();

            services.AddScoped<ReportPage>();
            services.AddScoped<ExpectedDepartureReport>();
            services.AddScoped<CurrentinHouseGuestReport>();
            services.AddScoped<OverstayedGuestReport>();
            services.AddScoped<RoomStatusReport>();
            services.AddScoped<RoomTransactionReport>();
            services.AddScoped<DailyRevenueReport>();

            services.AddScoped<ReservationPage>();
            services.AddScoped<AddReservationDialog>();

            services.AddScoped<LicenceDialog>();

            services.AddScoped<CardPage>();
            services.AddScoped<MasterCardPage>();
            services.AddScoped<BuildingCardPage>();
            services.AddScoped<FloorCardPage>();

            services.AddScoped<RoomPage>();
            services.AddScoped<LaundaryPage>();
            services.AddScoped<AddLaundaryServiceDialog>();

            services.AddScoped<SettledBookingReport>();

            return services;
        }
    }
}
