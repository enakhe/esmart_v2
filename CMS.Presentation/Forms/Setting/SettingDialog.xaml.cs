using ESMART.Presentation.Forms.Home;
using ESMART.Presentation.Forms.Setting.FinancialSetting;
using ESMART.Presentation.Forms.Setting.OperationalSetting;
using ESMART.Presentation.Forms.Setting.SystemSetup;
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
using System.Windows.Shapes;

namespace ESMART.Presentation.Forms.Setting
{
    /// <summary>
    /// Interaction logic for SettingDialog.xaml
    /// </summary>
    public partial class SettingDialog : Window
    {
        public SettingDialog()
        {
            InitializeComponent();

            var services = new ServiceCollection();
            DependencyInjection.ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            HotelInformationPage hotelInformationPage = serviceProvider.GetRequiredService<HotelInformationPage>();

            MainFrame.Navigate(hotelInformationPage);
        }

        private void SystemSetup_HotelInfo_Click(object sender, RoutedEventArgs e)
        {
            var services = new ServiceCollection();
            DependencyInjection.ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            HotelInformationPage hotelInformationPage = serviceProvider.GetRequiredService<HotelInformationPage>();

            MainFrame.Navigate(hotelInformationPage);
        }

        private void FinancialSetting_General_Click(object sender, RoutedEventArgs e)
        {
            var services = new ServiceCollection();
            DependencyInjection.ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            General generalPage = serviceProvider.GetRequiredService<General>();

            MainFrame.Navigate(generalPage);
        }

        private void SystemSetup_OpearationSetting_Click(object sender, RoutedEventArgs e)
        {
            var services = new ServiceCollection();
            DependencyInjection.ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            OperationSettingPage operationSettingPage = serviceProvider.GetRequiredService<OperationSettingPage>();

            MainFrame.Navigate(operationSettingPage);
        }
    }
}
