using ESMART.Presentation.Forms.Setting.Licence;
using ESMART.Presentation.Session;
using ESMART.Presentation.Utils;
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
using System.Windows.Shapes;

namespace ESMART.Presentation.Forms
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : Window
    {
        private IServiceProvider _serviceProvider;
        public SplashScreen()
        {
            InitializeComponent();
        }

        private async void SplashScreenForm_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(10000);
            this.Hide();

            InitializeServices();

            if (!TryLoadAndValidateLicense(out var licenseError))
            {
                MessageBox.Show(licenseError, "License Error", MessageBoxButton.OK, MessageBoxImage.Warning);

                var licenseForm = _serviceProvider.GetRequiredService<LicenceDialog>();
                bool? dialogResult = licenseForm.ShowDialog();

                if (!TryLoadAndValidateLicense(out licenseError))
                {
                    MessageBox.Show("License is still invalid. Application will now close.", "License Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    System.Windows.Application.Current.Shutdown();
                    return;
                }
            }

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
            this.Close();
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

        private bool TryLoadAndValidateLicense(out string errorMessage)
        {
            errorMessage = string.Empty;

            if (SecureFileHelper.TryLoadProductKey(out string hotelName, out string productKey, out DateTime expirationDate))
            {
                if (!LicenceHelper.ValidateProductKey(hotelName, productKey))
                {
                    errorMessage = "Invalid product key.";
                    return false;
                }

                if (expirationDate <= DateTime.Now)
                {
                    errorMessage = "License has expired.";
                    return false;
                }

                return true;
            }

            errorMessage = "No valid license found. Please enter a valid product key.";
            return false;
        }
    }
}
