using ESMART.Application.Common.Models;
using ESMART.Domain.Entities.Data;
using ESMART.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace ESMART.Infrastructure.Identity
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public IdentityService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<Result> CreatUserAsync(string userName, string password)
        {
            try
            {
                var user = new ApplicationUser
                {
                    UserName = userName,
                    Email = userName,
                };

                var result = await _userManager.CreateAsync(user, password);
                return (result.ToApplicationResult());
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured when creating user account. " + ex.Message);
            }
        }

        public async Task<Result> LoginUserAsync(string userName, string password)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(userName);
                if (user == null)
                {
                    IEnumerable<string> errors = new List<string> { "Invalid sign-in attempt, the email or password is incorrect" };
                    return Result.Failure(errors);
                }

                var result = await _userManager.CheckPasswordAsync(user!, password);
                if (!result)
                {
                    IEnumerable<string> errors = new List<string> { "Invalid sign-in attempt. The email or password is incorrect" };
                    return Result.Failure(errors);
                }
                return Result.Success();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured when trying to log in. " + ex.Message);
            }
        }

        public async Task TrySeedAsync()
        {
            var administratorRole = new IdentityRole(DefaultRoles.Administrator);
            var adminRole = new IdentityRole(DefaultRoles.Admin);
            var managerRole = new IdentityRole(DefaultRoles.Manager);

            if (_roleManager.Roles.All(r => r.Name != administratorRole.Name))
            {
                await _roleManager.CreateAsync(administratorRole);
            }

            if (_roleManager.Roles.All(r => r.Name != adminRole.Name))
            {
                await _roleManager.CreateAsync(adminRole);
            }

            if (_roleManager.Roles.All(r => r.Name != managerRole.Name))
            {
                await _roleManager.CreateAsync(managerRole);
            }

            var administrator = new ApplicationUser { UserName = "administrator@localhost", Email = "administrator@localhost" };
            var admin = new ApplicationUser { UserName = "admin@localhost", Email = "admin@localhost" };

            if (_userManager.Users.All(u => u.UserName != administrator.UserName))
            {
                await _userManager.CreateAsync(administrator, "Administrator1!");
                if (!string.IsNullOrWhiteSpace(administratorRole.Name))
                {
                    await _userManager.AddToRolesAsync(administrator, new[] { administratorRole.Name });
                }
            }
        }
    }
}
