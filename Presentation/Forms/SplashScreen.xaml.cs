using ESMART.Presentation.Forms.Setting.Licence;
using ESMART.Presentation.Session;
using ESMART.Presentation.Utils;
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
            Loaded += SplashScreenForm_Loaded;
        }

        private async void SplashScreenForm_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(10000);
            this.Hide();

            InitializeServices();

            if (!TryLoadAndValidateLicense(out var licenseError))
            {
                MessageBox.Show(licenseError, "License Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                if (!ShowLicenseForm()) 
                { 
                    Close(); 
                    return; 
                }
            }

            if (!ShowLoginForm()) 
            { 
                Close(); 
                return; 
            }

            this.Close();
        }

        private void InitializeServices()
        {
            var services = new ServiceCollection();
            DependencyInjection.ConfigureServices(services);
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

        private bool ShowLicenseForm()
        {
            var licenseForm = _serviceProvider.GetRequiredService<LicenceDialog>();

            if (licenseForm.ShowDialog() == true)
            {
                return true;
            }    

            return false;
        }

        private bool ShowLoginForm()
        {
            MainWindow mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            if (mainWindow.ShowDialog() == true)
            {
                return true;
            }

            return false;
        }

    }
}
