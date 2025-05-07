using ESMART.Domain.Entities.Data;
using ESMART.Domain.ViewModels.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Application.Common.Interface
{
    public interface IApplicationRole
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
        Task<bool> IsUserInAnyRole(ApplicationUser user);
        Task<bool> RoleHasAnyUsers(ApplicationRole role);
        Task<int> GetNumberOfUsersInRole(ApplicationRole role);
        Task<int> GetNumberOfRolesForUser(ApplicationUser user);
    }
}
