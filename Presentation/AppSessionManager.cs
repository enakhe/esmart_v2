using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Windows;

namespace ESMART.Presentation
{
    public static class AppSessionManager
    {

        public static void LogoutToLogin()
        {
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            DependencyInjection.ConfigureServices(services, configuration);
            var serviceProvider = services.BuildServiceProvider();

            // Show login screen again
            var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            // Close all open windows except LoginWindow
            foreach (Window win in System.Windows.Application.Current.Windows)
            {
                if (win is not MainWindow)
                    win.Close();
            }
        }
    }
}
