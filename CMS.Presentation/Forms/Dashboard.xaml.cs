using ESMART.Application.Common.Interface;
using ESMART.Presentation.Forms.FrontDesk.Booking;
using ESMART.Presentation.Forms.FrontDesk.Guest;
using ESMART.Presentation.Forms.Home;
using ESMART.Presentation.Forms.RoomSetting;
using ESMART.Presentation.Forms.Setting;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace ESMART.Presentation.Forms
{
    public partial class Dashboard : Window
    {
        private bool _isLoading;
        private readonly IHotelSettingsService _hotelSettingsService;
        public Dashboard(IHotelSettingsService hotelSettingsService)
        {
            _hotelSettingsService = hotelSettingsService;
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

        private void OpenSidebar_Click(object sender, RoutedEventArgs e)
        {
            if (this.sideBar.Width < 250)
                this.sideBar.Width = 250;
            else
                this.sideBar.Width = 50;
        }

        private async void Window_Activated(object sender, EventArgs e)
        {
            await LoadData();
        }
    }
}
