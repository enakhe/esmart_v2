using ESMART.Presentation.Forms.Home;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ESMART.Presentation.Forms.Reports
{
    /// <summary>
    /// Interaction logic for ReportPage.xaml
    /// </summary>
    public partial class ReportPage : Page
    {
        private IServiceProvider _serviceProvider;
        public ReportPage()
        {
            InitializeComponent();
        }

        private void InitializeServices()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var services = new ServiceCollection();
            DependencyInjection.ConfigureServices(services, configuration);
            _serviceProvider = services.BuildServiceProvider();
        }

        public void ExpectedDepartureReport_Click(object sender, RoutedEventArgs e)
        {
            InitializeServices();

            ExpectedDepartureReport expectedDepartureReport = _serviceProvider.GetRequiredService<ExpectedDepartureReport>();

            MainFrame.Navigate(expectedDepartureReport);
        }

        public void CurrentInHouseGuestReport_Click(object sender, RoutedEventArgs e)
        {
            InitializeServices();

            CurrentinHouseGuestReport currentinHouseGuestReport = _serviceProvider.GetRequiredService<CurrentinHouseGuestReport>();

            MainFrame.Navigate(currentinHouseGuestReport);
        }

        public void OverstayedGuestReport_Click(object sender, RoutedEventArgs e)
        {
            InitializeServices();

            OverstayedGuestReport overstayedGuestReport = _serviceProvider.GetRequiredService<OverstayedGuestReport>();

            MainFrame.Navigate(overstayedGuestReport);
        }

        public void RoomStatusReport_Click(object sender, RoutedEventArgs e)
        {
            InitializeServices();

            RoomStatusReport roomStatusReport = _serviceProvider.GetRequiredService<RoomStatusReport>();

            MainFrame.Navigate(roomStatusReport);
        }

        public void RoomTransactionReport_Click(object sender, RoutedEventArgs e)
        {
            InitializeServices();

            RoomTransactionReport roomTransactionReport = _serviceProvider.GetRequiredService<RoomTransactionReport>();

            MainFrame.Navigate(roomTransactionReport);
        }
    }
}
