using ESMART.Application;
using ESMART.Domain;
using ESMART.Domain.Entities.Data;
using ESMART.Infrastructure;
using ESMART.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ESMART.Presentation
{
    public class DependencyInjection
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddDebug();
                loggingBuilder.AddConsole();
            });

            services.AddDbContextFactory<ApplicationDbContext>(options =>
            {
                options.UseSqlServer("Server=ENAKHE;Database=ESMART;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");
            });

            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();


            services.AddTransient<IUserEmailStore<ApplicationUser>, UserStore<ApplicationUser, IdentityRole, ApplicationDbContext>>();
            services.AddTransient<IUserStore<ApplicationUser>, UserStore<ApplicationUser, IdentityRole, ApplicationDbContext>>();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            });

            // Domain
            DomainDependencyInjection.AddDomainServices(services);

            // Application
            ApplicationDependencyInjection.AddApplicationServices(services);

            // Presentation
            PresentationDependencyInjection.AddPresentationServices(services);

            // Infrastructure
            InfrastructureDependencyInjection.AddInfrastructureServices(services);
        }
    }
}
