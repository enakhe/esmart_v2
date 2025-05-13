using ESMART.Presentation.Forms.Home;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
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
        public ReportPage()
        {
            InitializeComponent();
        }

        public void ExpectedDepartureReport_Click(object sender, RoutedEventArgs e)
        {
            var services = new ServiceCollection();
            DependencyInjection.ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            ExpectedDepartureReport expectedDepartureReport = serviceProvider.GetRequiredService<ExpectedDepartureReport>();

            MainFrame.Navigate(expectedDepartureReport);
        }

        public void CurrentInHouseGuestReport_Click(object sender, RoutedEventArgs e)
        {
            var services = new ServiceCollection();
            DependencyInjection.ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            CurrentinHouseGuestReport currentinHouseGuestReport = serviceProvider.GetRequiredService<CurrentinHouseGuestReport>();

            MainFrame.Navigate(currentinHouseGuestReport);
        }
    }
}
