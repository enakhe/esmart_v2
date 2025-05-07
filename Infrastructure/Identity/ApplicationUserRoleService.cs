#nullable disable

using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Models;
using ESMART.Domain.Entities.Data;
using ESMART.Domain.ViewModels.Data;
using ESMART.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ESMART.Infrastructure.Identity
{
    public class ApplicationUserRoleService(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, IDbContextFactory<ApplicationDbContext> contextFactory) : IApplicationUserRoleRepository
    {
        private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory = contextFactory;

        public async Task AddRole(ApplicationRole role)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                await _roleManager.CreateAsync(role);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured when adding role. " + ex.Message);
            }
        }

        // Add application user
        public async Task<Result> AddUser(ApplicationUser user, string password)
        {
            try
            {
                var result = await _userManager.CreateAsync(user, password);
                return (result.ToApplicationResult(user));
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured when adding user. " + ex.Message);
            }
        }

        public async Task<ApplicationRole> GetRoleById(string id)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(id);
                return role;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured when getting role by id. " + ex.Message);
            }
        }

        // Get user by id
        public async Task<ApplicationUser> GetUserById(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                return user;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured when getting user by id. " + ex.Message);
            }
        }

        public async Task<ApplicationRole> GetRoleByName(string name)
        {
            try
            {
                var role = await _roleManager.FindByNameAsync(name);
                return role;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured when getting role by name. " + ex.Message);
            }
        }

        public async Task<ApplicationUser> GetUserByName(string name)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(name);
                return user;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured when getting user by name. " + ex.Message);
            }
        }

        public async Task UpdateRole(ApplicationRole role)
        {
            try
            {
                await _roleManager.UpdateAsync(role);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured when updating role. " + ex.Message);
            }
        }

        // Update user
        public async Task UpdateUser(ApplicationUser user)
        {
            try
            {
                await _userManager.UpdateAsync(user);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured when updating user. " + ex.Message);
            }
        }

        public async Task DeleteRole(ApplicationRole role)
        {
            try
            {
                await _roleManager.DeleteAsync(role);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured when deleting role. " + ex.Message);
            }
        }

        // Delete user
        public async Task DeleteUser(ApplicationUser user)
        {
            try
            {
                await _userManager.DeleteAsync(user);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured when deleting user. " + ex.Message);
            }
        }

        // Update user password
        public async Task UpdateUserPassword(ApplicationUser user, string newPassword)
        {
            //try
            //{
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
                if (!result.Succeeded)
                {
                    throw new Exception("An error occured when updating user password. " + string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            //}
            //catch (Exception ex)
            //{
            //    throw new Exception("An error occured when updating user password. " + ex.Message);
            //}
        }

        public async Task<List<ApplicationRoleViewModel>> GetAllRoles()
        {
            try
            {
                var roles = await _roleManager.Roles
                    .Where(r => r.Name != "Administrator")
                    .Select(r => new ApplicationRoleViewModel
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Description = r.Description,
                        NormalizedName = r.NormalizedName,
                        ConcurrencyStamp = r.ConcurrencyStamp,
                        DateCreated = r.DateCreated,
                        NoOfUser = r.NoOfUser
                    })
                    .OrderBy(r => r.Name)
                    .ToListAsync();

                return roles;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured when getting all roles. " + ex.Message);
            }
        }

        // Get all users
        public async Task<List<ApplicationUserViewModel>> GetAllUsers()
        {
            try
            {
                var users = await _userManager.Users
                    .Where(u => u.UserName != "administrator@localhost")
                    .Select(u => new ApplicationUserViewModel
                    {
                        Id = u.Id,
                        UserName = u.UserName,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        MiddleName = u.MiddleName,
                        Email = u.Email,
                        PhoneNumber = u.PhoneNumber,
                        DateCreated = u.DateCreated,
                        Role = _userManager.GetRolesAsync(u).Result.FirstOrDefault()
                    })
                    .OrderBy(u => u.FirstName)
                    .ToListAsync();

                return users;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured when getting all users. " + ex.Message);
            }
        }

        public async Task<bool> RoleExists(string roleName)
        {
            try
            {
                var role = await _roleManager.RoleExistsAsync(roleName);
                return role;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured when checking if role exists. " + ex.Message);
            }
        }

        // Assign role to user
        public async Task AssignRoleToUser(ApplicationUser user, ApplicationRole role)
        {
            try
            {
                await _userManager.AddToRoleAsync(user, role.Name);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured when assigning role to user. " + ex.Message);
            }
        }

        // Remove role from user
        public async Task RemoveRoleFromUser(ApplicationUser user, ApplicationRole role)
        {
            try
            {
                await _userManager.RemoveFromRoleAsync(user, role.Name);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured when removing role from user. " + ex.Message);
            }
        }

        // Update user role
        public async Task UpdateUserRole(ApplicationUser user, ApplicationRole role)
        {
            try
            {
                var roles = await _userManager.GetRolesAsync(user);
                foreach (var r in roles)
                {
                    await _userManager.RemoveFromRoleAsync(user, r);
                }
                await _userManager.AddToRoleAsync(user, role.Name);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured when updating user role. " + ex.Message);
            }
        }

        // Get all users in a role
        public async Task<List<ApplicationUser>> GetUsersInRole(ApplicationRole role)
        {
            try
            {
                var users = await _userManager.GetUsersInRoleAsync(role.Name);
                return users.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured when getting all users in a role. " + ex.Message);
            }
        }

        // Get all roles for a user
        public async Task<List<ApplicationRole>> GetRolesForUser(ApplicationUser user)
        {
            try
            {
                var roles = await _userManager.GetRolesAsync(user);
                return roles.Select(r => _roleManager.FindByNameAsync(r).Result).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured when getting all roles for a user. " + ex.Message);
            }
        }

        // Check if user is in role
        public async Task<bool> IsUserInRole(ApplicationUser user, ApplicationRole role)
        {
            try
            {
                var result = await _userManager.IsInRoleAsync(user, role.Name);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured when checking if user is in role. " + ex.Message);
            }
        }

        // Check if user is in any role
        public async Task<bool> IsUserInAnyRole(ApplicationUser user)
        {
            try
            {
                var roles = await _userManager.GetRolesAsync(user);
                return roles.Count() > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured when checking if user is in any role. " + ex.Message);
            }
        }

        // Check if role has any users
        public async Task<bool> RoleHasAnyUsers(ApplicationRole role)
        {
            try
            {
                var users = await _userManager.GetUsersInRoleAsync(role.Name);
                return users.Count() > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured when checking if role has any users. " + ex.Message);
            }
        }

        // Get number of users in a role
        public async Task<int> GetNumberOfUsersInRole(ApplicationRole role)
        {
            try
            {
                var users = await _userManager.GetUsersInRoleAsync(role.Name);
                return users.Count();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured when getting number of users in a role. " + ex.Message);
            }
        }

        // Get number of roles for a user
        public async Task<int> GetNumberOfRolesForUser(ApplicationUser user)
        {
            try
            {
                var roles = await _userManager.GetRolesAsync(user);
                return roles.Count();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured when getting number of roles for a user. " + ex.Message);
            }
        }
    }
}
