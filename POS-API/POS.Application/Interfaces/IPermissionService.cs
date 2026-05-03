using POS.Application.DTOs.Permission;

namespace POS.Application.Interfaces
{
    public interface IPermissionService
    {
        Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync();
        Task AssignPermissionsToRoleAsync(int roleId, List<int> permissionIds);
        Task<IEnumerable<PermissionDto>> GetPermissionsByRoleIdAsync(int roleId);
    }
}
