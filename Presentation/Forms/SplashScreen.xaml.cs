using ESMART.Application.Common.Interface;
using ESMART.Domain.Entities.Configuration;
using ESMART.Infrastructure.Repositories.Configuration;
using ESMART.Presentation.Forms.Setting.Licence;
using ESMART.Presentation.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Windows;

namespace ESMART.Presentation.Forms
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : Window
    {
        private IServiceProvider _serviceProvider;
        private readonly IBackupRepository _backupRepository;
        private readonly IHotelSettingsService _hotelSettingsService;
        public SplashScreen(IHotelSettingsService hotelSettingsService, IBackupRepository backupRepository)
        {
            _hotelSettingsService = hotelSettingsService;
            _backupRepository = backupRepository;
            InitializeComponent();
        }

        private async void SplashScreenForm_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(10000);
            await BackUpAsync();
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

        public async Task BackUpAsync()
        {
            UserBackupSettings settings = await _backupRepository.GetBackupSettingsAsync();

            if (settings != null)
            {
                if (BackupRepository.IsTimeToBackup(settings))
                {
                    await CreateBackup();
                }
            }
        }

        public async Task CreateBackup()
        {
            var backupFile = BackupRepository.CreateBackup();
            var hotel = await _hotelSettingsService.GetHotelInformation();
            if (hotel != null)
            {
                var zippedFile = BackupRepository.ZipFiles(backupFile);
                var result = await BackupRepository.UploadBackupAsync(zippedFile, hotel.Name);

                if (result.Success)
                {
                    var userBackUp = await _backupRepository.GetBackupSettingsAsync();
                    userBackUp.LastBackup = DateTime.Now;
                    await _backupRepository.UpdateBackupSettingsAsync(userBackUp);
                }
            }
        }
    }
}
