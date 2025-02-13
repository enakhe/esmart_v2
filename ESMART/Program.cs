using Microsoft.Extensions.Hosting;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Presentation
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            var host = CreateHostBuilder().Build();
            var app = host.Services.GetRequiredService<App>();
            app.Run();
        }

        public static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlServer("YourConnectionStringHere"));

                    services.AddSingleton<App>();
                    services.AddSingleton<MainWindow>();
                });
    }
}
