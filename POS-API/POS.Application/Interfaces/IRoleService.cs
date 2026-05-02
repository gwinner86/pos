using POS.Application.DTOs.Role;

namespace POS.Application.Interfaces
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleResponse>> GetRolesAsync(Guid tenantId);
        Task<RoleResponse?> GetRoleByIdAsync(int roleId, Guid tenantId);
        Task<int> CreateRoleAsync(CreateRoleDto request, Guid tenantId);
        Task<bool> UpdateRoleAsync(int roleId, UpdateRoleDto request, Guid tenantId);
        Task<bool> DeleteRoleAsync(int roleId, Guid tenantId);
    }
}
