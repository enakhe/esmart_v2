using ESMART.Application.Common.Interface;
using ESMART.Domain.Entities.Data;
using ESMART.Presentation.Forms.Cards;
using ESMART.Presentation.Forms.FrontDesk.Booking;
using ESMART.Presentation.Forms.FrontDesk.Guest;
using ESMART.Presentation.Forms.FrontDesk.Reservation;
using ESMART.Presentation.Forms.FrontDesk.Room;
using ESMART.Presentation.Forms.Home;
using ESMART.Presentation.Forms.Reports;
using ESMART.Presentation.Forms.RoomSetting;
using ESMART.Presentation.Forms.Setting;
using ESMART.Presentation.Forms.UserSetting;
using ESMART.Presentation.Session;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace ESMART.Presentation.Forms
{
    public partial class Dashboard : Window
    {
        private bool _isLoading;
        private readonly IHotelSettingsService _hotelSettingsService;
        private readonly IApplicationUserRoleRepository _userService;
        private readonly UserManager<ApplicationUser> _userManager;
        public Dashboard(IHotelSettingsService hotelSettingsService, IApplicationUserRoleRepository userService, UserManager<ApplicationUser> userManager)
        {
            _hotelSettingsService = hotelSettingsService;
            _userService = userService;
            _userManager = userManager;
            InitializeComponent();

            var services = new ServiceCollection();
            DependencyInjection.ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            IndexPage indexPage = serviceProvider.GetRequiredService<IndexPage>();

            MainFrame.Navigate(indexPage);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                LoaderGrid.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private async Task LoadData()
        {
            try
            {
                var hotel = await _hotelSettingsService.GetHotelInformation();
                if (hotel != null)
                {
                    txtHotelName.Text = hotel.Name;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            var services = new ServiceCollection();
            DependencyInjection.ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            IndexPage indexPage = serviceProvider.GetRequiredService<IndexPage>();

            MainFrame.Navigate(indexPage);
        }

        private void GuestButton_Click(Object sender, RoutedEventArgs e)
        {
            var services = new ServiceCollection();
            DependencyInjection.ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            GuestPage guestPage = serviceProvider.GetRequiredService<GuestPage>();

            MainFrame.Navigate(guestPage);
        }

        private void BookingButton_Click(object sender, RoutedEventArgs e)
        {
            var services = new ServiceCollection();
            DependencyInjection.ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            BookingPage bookingPage = serviceProvider.GetRequiredService<BookingPage>();

            MainFrame.Navigate(bookingPage);
        }

        private void RoomSettingButton_Click(object sender, RoutedEventArgs e)
        {
            var services = new ServiceCollection();
            DependencyInjection.ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            RoomSettingPage roomSettingPage = serviceProvider.GetRequiredService<RoomSettingPage>();

            MainFrame.Navigate(roomSettingPage);
        }

        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            var services = new ServiceCollection();
            DependencyInjection.ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            SettingDialog settingPage = serviceProvider.GetRequiredService<SettingDialog>();
            settingPage.ShowDialog();
        }

        private void UserSettingButton_Click(object sender, RoutedEventArgs e)
        {
            var services = new ServiceCollection();
            DependencyInjection.ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            UserSettingPage userSettingPage = serviceProvider.GetRequiredService<UserSettingPage>();
            MainFrame.Navigate(userSettingPage);
        }

        private void ReportButton_Click(object sender, RoutedEventArgs e)
        {
            var services = new ServiceCollection();
            DependencyInjection.ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            ReportPage reportPage = serviceProvider.GetRequiredService<ReportPage>();
            MainFrame.Navigate(reportPage);
        }

        private void ReservationButton_Click(object sender, RoutedEventArgs e)
        {
            var services = new ServiceCollection();
            DependencyInjection.ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            ReservationPage reservationPage = serviceProvider.GetRequiredService<ReservationPage>();
            MainFrame.Navigate(reservationPage);
        }

        //Navigate to Card setting page
        private void CarSettingButton_Click(object sender, RoutedEventArgs e)
        {
            var services = new ServiceCollection();
            DependencyInjection.ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            CardPage cardPage = serviceProvider.GetRequiredService<CardPage>();
            MainFrame.Navigate(cardPage);
        }

        private void RoomButton_Click(Object sender, RoutedEventArgs e)
        {
            var services = new ServiceCollection();
            DependencyInjection.ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            RoomPage roomPage = serviceProvider.GetRequiredService<RoomPage>();
            MainFrame.Navigate(roomPage);
        }

        private void OpenSidebar_Click(object sender, RoutedEventArgs e)
        {
            if (this.sideBar.Width < 250)
                this.sideBar.Width = 250;
            else
                this.sideBar.Width = 50;
        }

        private async void Window_Activated(object sender, EventArgs e)
        {
            //await ApplyAuthorization();
            await LoadData();
        }

        private async Task ApplyAuthorization()
        {
            try
            {
                var userId = AuthSession.CurrentUser?.Id;

                if (userId != null)
                {
                    var user = await _userService.GetUserById(userId);
                    if (user != null)
                    {
                        bool isAdmin = await _userManager.IsInRoleAsync(user, DefaultRoles.Administrator.ToString()) || await _userManager.IsInRoleAsync(user, DefaultRoles.Admin.ToString());
                        if (isAdmin)
                        {
                            AdminControls.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            AdminControls.Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
