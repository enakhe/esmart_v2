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

            var administratorRole = new ApplicationRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = DefaultRoles.Administrator.ToString(),
                Description = "Administrator Role",
            };

            if (_roleManager.Roles.All(r => r.Name != administratorRole.Name))
                await _roleManager.CreateAsync(administratorRole);

            var adminRole = new ApplicationRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = DefaultRoles.Admin.ToString(),
                Description = "Admin Role",
            };

            if (_roleManager.Roles.All(r => r.Name != adminRole.Name))
                await _roleManager.CreateAsync(adminRole);

            var managerRole = new ApplicationRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = DefaultRoles.Manager.ToString(),
                Description = "Manager Role",
            };

            if (_roleManager.Roles.All(r => r.Name != managerRole.Name))
                await _roleManager.CreateAsync(managerRole);

            var receptionistRole = new ApplicationRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = DefaultRoles.Receptionist.ToString(),
                Description = "Receptionist Role",
            };

            if (_roleManager.Roles.All(r => r.Name != receptionistRole.Name))
                await _roleManager.CreateAsync(receptionistRole);

            var accountantRole = new ApplicationRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = DefaultRoles.Accountant.ToString(),
                Description = "Accountant Role",
            };

            if (_roleManager.Roles.All(r => r.Name != accountantRole.Name))
                await _roleManager.CreateAsync(accountantRole);

            var barRole = new ApplicationRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = DefaultRoles.Bar.ToString(),
                Description = "Bar Role",
            };

            if (_roleManager.Roles.All(r => r.Name != barRole.Name))
                await _roleManager.CreateAsync(barRole);

            var restaurantRole = new ApplicationRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = DefaultRoles.Restaurant.ToString(),
                Description = "Restaurant Role",
            };

            if (_roleManager.Roles.All(r => r.Name != restaurantRole.Name))
                await _roleManager.CreateAsync(restaurantRole);

            var storeKeeperRole = new ApplicationRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = DefaultRoles.StoreKeeper.ToString(),
                Description = "Store Keeper Role",
            };

            if (_roleManager.Roles.All(r => r.Name != storeKeeperRole.Name))
                await _roleManager.CreateAsync(storeKeeperRole);

            var auditorRole = new ApplicationRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = DefaultRoles.Auditor.ToString(),
                Description = "Auditor Role",
            };

            if (_roleManager.Roles.All(r => r.Name != auditorRole.Name))
                await _roleManager.CreateAsync(auditorRole);

            var administrator = new ApplicationUser { 
                UserName = "administrator@localhost", 
                Email = "administrator@localhost", 
                FirstName = "Super", 
                LastName = "Administrator", 
                MiddleName = "User", 
                PhoneNumber = "+2349069477106", 
                RoleId = administratorRole.Id 
            };

            if (_userManager.Users.All(u => u.UserName != administrator.UserName))
            {
                await _userManager.CreateAsync(administrator, "Administrator1!");
                if (!string.IsNullOrWhiteSpace(administratorRole.Name))
                {
                    await _userManager.AddToRolesAsync(administrator, [administratorRole.Name]);
                }
            }

            var admin = new ApplicationUser { 
                UserName = "admin@localhost", 
                Email = "admin@localhost", 
                FirstName = "Hotel", 
                LastName = "Admin", 
                MiddleName = "User", 
                RoleId = adminRole.Id 
            };

            if (_userManager.Users.All(u => u.UserName != admin.UserName))
            {
                await _userManager.CreateAsync(admin, "Admin1!");
                if (!string.IsNullOrWhiteSpace(adminRole.Name))
                {
                    await _userManager.AddToRolesAsync(admin, [adminRole.Name]);
                }
            }
        }
    }
}
