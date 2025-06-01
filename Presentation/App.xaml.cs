using ESMART.Application.Common.Interface;
using ESMART.Infrastructure.Data;
using ESMART.Infrastructure.Identity;
using ESMART.Infrastructure.Repositories.Configuration;
using ESMART.Infrastructure.Services;
using Hangfire;
using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Windows;

namespace ESMART.Presentation
{
    public partial class App : System.Windows.Application
    {
        private readonly IConfiguration _configuration;

        public App()
        {
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ESMART");
            string appSettingsPath = Path.Combine(appDataPath, "appsettings.json");

            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
                File.SetAttributes(appDataPath, FileAttributes.Hidden);
            }

            if (!File.Exists(appSettingsPath))
            {
                string defaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
                if (File.Exists(defaultPath))
                {
                    File.Copy(defaultPath, appSettingsPath);
                }
                else
                {
                    throw new FileNotFoundException("Missing appsettings.json in both AppData and application directory.");
                }
            }

            var builder = new ConfigurationBuilder()
                .SetBasePath(appDataPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();

        }

        protected async override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();

            DependencyInjection.ConfigureServices(services, _configuration);
            

            var serviceProvider = services.BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.Migrate();
            }

            var nightlyService = serviceProvider.GetRequiredService<NightlyRoomChargeService>();
            await nightlyService.PostNightlyRoomChargesAsync();

            var identityService = serviceProvider.GetRequiredService<IdentityService>();
            await identityService.TrySeedAsync();

            var stockKeepingRepository = serviceProvider.GetRequiredService<IStockKeepingRepository>();
            await stockKeepingRepository.SeedDefaultMenuItemCategoriesAsync();

            var hotelSettingService = serviceProvider.GetRequiredService<HotelSettingsService>();
            await hotelSettingService.SeedHotelSettingAsync();
            await hotelSettingService.SeedHotelInformation();

            Presentation.Forms.SplashScreen splashScreen = serviceProvider.GetRequiredService<Presentation.Forms.SplashScreen>();
            splashScreen.Show();
        }
    }

}
