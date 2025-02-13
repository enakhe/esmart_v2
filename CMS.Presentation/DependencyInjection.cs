using ESMART.Application;
using ESMART.Domain;
using ESMART.Infrastructure;
using ESMART.Infrastructure.Data;
using ESMART.Infrastructure.Identity;
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

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer("Server=ENAKHE;Database=ESMART;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"));

            services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddSignInManager<SignInManager<ApplicationUser>>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

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
