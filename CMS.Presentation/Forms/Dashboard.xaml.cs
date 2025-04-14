using ESMART.Presentation.Forms.FrontDesk.Booking;
using ESMART.Presentation.Forms.FrontDesk.Guest;
using ESMART.Presentation.Forms.Home;
using ESMART.Presentation.Forms.RoomSetting;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace ESMART.Presentation.Forms
{
    public partial class Dashboard : Window
    {
        private bool _isLoading;
        public Dashboard()
        {
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
    }
}
