using ESMART.Presentation.Forms.Setting.FinancialSetting;
using ESMART.Presentation.Forms.Setting.OperationalSetting;
using ESMART.Presentation.Forms.Setting.SystemSetup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Windows;

namespace ESMART.Presentation.Forms.Setting
{
    /// <summary>
    /// Interaction logic for SettingDialog.xaml
    /// </summary>
    public partial class SettingDialog : Window
    {
        private IServiceProvider _serviceProvider;
        public SettingDialog()
        {
            InitializeComponent();

            LoadHotelPage();
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

        private void LoadHotelPage()
        {
            InitializeServices();

            HotelInformationPage hotelInformationPage = _serviceProvider.GetRequiredService<HotelInformationPage>();

            MainFrame.Navigate(hotelInformationPage);
        }

        private void SystemSetup_HotelInfo_Click(object sender, RoutedEventArgs e)
        {
            LoadHotelPage();
        }

        private void FinancialSetting_General_Click(object sender, RoutedEventArgs e)
        {
            InitializeServices();

            General generalPage = _serviceProvider.GetRequiredService<General>();

            MainFrame.Navigate(generalPage);
        }

        private void SystemSetup_OpearationSetting_Click(object sender, RoutedEventArgs e)
        {
            InitializeServices();

            OperationSettingPage operationSettingPage = _serviceProvider.GetRequiredService<OperationSettingPage>();

            MainFrame.Navigate(operationSettingPage);
        }

        private void FinancialSetting_BankAccount(object sender, RoutedEventArgs e)
        {
            InitializeServices();

            BankAccountPage bankAccountPage = _serviceProvider.GetRequiredService<BankAccountPage>();
            MainFrame.Navigate(bankAccountPage);
        }
    }
}
