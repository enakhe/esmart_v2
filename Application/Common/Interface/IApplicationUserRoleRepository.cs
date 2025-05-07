using ESMART.Application.Common.Models;
using ESMART.Domain.Entities.Data;
using ESMART.Domain.ViewModels.Data;

namespace ESMART.Application.Common.Interface
{
    public interface IApplicationUserRoleRepository
    {
        Task AddRole(ApplicationRole role);
        Task<ApplicationRole> GetRoleById(string id);
        Task<ApplicationRole> GetRoleByName(string name);
        Task UpdateRole(ApplicationRole role);
        Task DeleteRole(ApplicationRole role);
        Task<List<ApplicationRoleViewModel>> GetAllRoles();
        Task<bool> RoleExists(string roleName);
        Task AssignRoleToUser(ApplicationUser user, ApplicationRole role);
        Task RemoveRoleFromUser(ApplicationUser user, ApplicationRole role);
        Task<List<ApplicationUser>> GetUsersInRole(ApplicationRole role);
        Task<List<ApplicationRole>> GetRolesForUser(ApplicationUser user);
        Task<bool> IsUserInRole(ApplicationUser user, ApplicationRole role);
        Task UpdateUserRole(ApplicationUser user, ApplicationRole role);
        Task UpdateUserPassword(ApplicationUser user, string newPassword);
        Task<bool> IsUserInAnyRole(ApplicationUser user);
        Task<bool> RoleHasAnyUsers(ApplicationRole role);
        Task<int> GetNumberOfUsersInRole(ApplicationRole role);
        Task<int> GetNumberOfRolesForUser(ApplicationUser user);

        Task<Result> AddUser(ApplicationUser user, string password);
        Task<ApplicationUser> GetUserById(string id);
        Task<ApplicationUser> GetUserByName(string name);
        Task UpdateUser(ApplicationUser user);
        Task DeleteUser(ApplicationUser user);
        Task<List<ApplicationUserViewModel>> GetAllUsers();
    }
}
