using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ESMART.Presentation
{
    public static class AppSessionManager
    {

        public static void LogoutToLogin()
        {
            var services = new ServiceCollection();
            DependencyInjection.ConfigureServices(services);
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
