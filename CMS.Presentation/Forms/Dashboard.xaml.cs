using ESMART.Presentation.Forms.FrontDesk.Guest;
using ESMART.Presentation.Forms.Home;
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
            MainFrame.Navigate(new IndexPage());
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
            MainFrame.Navigate(new IndexPage());
        }

        private void GuestButton_Click(Object sender, RoutedEventArgs e)
        {
            var services = new ServiceCollection();
            DependencyInjection.ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();
            GuestPage guestPage = serviceProvider.GetRequiredService<GuestPage>();
            MainFrame.Navigate(guestPage);
        }
    }
}
