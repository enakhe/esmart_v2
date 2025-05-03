using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Models;
using ESMART.Domain.Entities.Data;
using Microsoft.AspNetCore.Identity;

namespace ESMART.Infrastructure.Identity
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public IdentityService(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
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
                return (result.ToApplicationResult(user));
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

                return Result.Success(user);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured when trying to log in. " + ex.Message);
            }
        }


        public async Task TrySeedAsync()
        {
            var administrator = new ApplicationUser { UserName = "administrator@localhost", Email = "administrator@localhost", FirstName = "Super", LastName = "Administrator", MiddleName = " " };

            if (_userManager.Users.All(u => u.UserName != administrator.UserName))
                await _userManager.CreateAsync(administrator, "Administrator1!");

            var administratorRole = new ApplicationRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = DefaultRoles.Administrator.ToString(),
                ManagerId = administrator.Id,
                Manager = administrator,
                Description = "Administrator Role",

            };

            if (_roleManager.Roles.All(r => r.Name != administratorRole.Name))
                await _roleManager.CreateAsync(administratorRole);

            var admin = new ApplicationUser { UserName = "admin@localhost", Email = "admin@localhost", FirstName = "Admin", LastName = "User", MiddleName = " " };

            if (_userManager.Users.All(u => u.UserName != administrator.UserName))
            {
                if (!string.IsNullOrWhiteSpace(administratorRole.Name))
                {
                    await _userManager.AddToRolesAsync(administrator, [administratorRole.Name]);
                }
            }
        }
    }
}
