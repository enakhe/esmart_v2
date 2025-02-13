using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CMS.Presentation
{
    public partial class App : System.Windows.Application
    {
        private readonly MainWindow _mainWindow;

        public App(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            _mainWindow.Show();
            base.OnStartup(e);
        }
    }

}
