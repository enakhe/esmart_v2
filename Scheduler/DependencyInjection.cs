using ESMART.Application;
using ESMART.Infrastructure;
using ESMART.Infrastructure.Data;
using ESMART.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler
{
    public class DependencyInjection
    {
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContextFactory<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure());
            });

            services.AddSingleton<NightlyRoomChargeService>();
            services.AddLogging();

            // Infrastructure
            InfrastructureDependencyInjection.AddInfrastructureServices(services);
            // Application
            ApplicationDependencyInjection.AddApplicationServices(services);
        }
    }
}
