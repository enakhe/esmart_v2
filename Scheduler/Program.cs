using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ESMART.Infrastructure.Data;
using ESMART.Infrastructure.Services;
using ESMART.Application.Common.Interface;
using ESMART.Infrastructure.Repositories.RoomSetting;
using ESMART.Infrastructure;
using Microsoft.Extensions.Configuration;
using ESMART.Infrastructure.Repositories.FrontDesk;
using ESMART.Infrastructure.Repositories.Transaction;
using Microsoft.EntityFrameworkCore;

namespace Scheduler
{
    public class Program
    {
        private readonly IConfiguration _configuration;

        public Program()
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

        public async Task Run()
        {
            Console.WriteLine("Executing nightly room charges...");
            try
            {
                var services = new ServiceCollection();

                DependencyInjection.ConfigureServices(services, _configuration);

                var serviceProvider = services.BuildServiceProvider();

                var roomChargeService = serviceProvider.GetRequiredService<NightlyRoomChargeService>();
                await roomChargeService.PostNightlyRoomChargesAsync();

                Console.WriteLine("Room charges successfully posted.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static async Task Main()
        {
            var promram = new Program();
            await promram.Run();
        }
    }
}
