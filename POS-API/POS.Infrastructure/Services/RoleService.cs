using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs.Role;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Infrastructure.Persistence;

namespace POS.Infrastructure.Services
{
    public class RoleService : IRoleService
    {
        private readonly ApplicationDbContext _context;

        public RoleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RoleResponse>> GetRolesAsync(Guid tenantId)
        {
            return await _context.Roles
                .Where(r => r.TenantId == tenantId)
                .Select(r => new RoleResponse
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName,
                    Description = r.Description,
                    TenantId = r.TenantId,
                    CompanyId = r.CompanyId
                })
                .ToListAsync();
        }

        public async Task<RoleResponse?> GetRoleByIdAsync(int roleId, Guid tenantId)
        {
            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleId == roleId && r.TenantId == tenantId);

            if (role == null) return null;

            return new RoleResponse
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                Description = role.Description,
                TenantId = role.TenantId,
                CompanyId = role.CompanyId
            };
        }

        public async Task<int> CreateRoleAsync(CreateRoleDto request, Guid tenantId)
        {
            // Check for uniqueness within tenant
            var exists = await _context.Roles.AnyAsync(r => r.TenantId == tenantId && r.RoleName == request.RoleName);
            if (exists)
            {
                throw new InvalidOperationException($"Role '{request.RoleName}' already exists for this tenant.");
            }

            var role = new Role
            {
                RoleName = request.RoleName,
                Description = request.Description,
                TenantId = tenantId,
                CompanyId = request.CompanyId
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            return role.RoleId;
        }

        public async Task<bool> UpdateRoleAsync(int roleId, UpdateRoleDto request, Guid tenantId)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId && r.TenantId == tenantId);
            if (role == null) return false;

            // Check uniqueness if name changing
            if (role.RoleName != request.RoleName)
            {
                 var exists = await _context.Roles.AnyAsync(r => r.TenantId == tenantId && r.RoleName == request.RoleName);
                 if (exists) throw new InvalidOperationException($"Role '{request.RoleName}' already exists.");
            }

            role.RoleName = request.RoleName;
            role.Description = request.Description;

            _context.Roles.Update(role);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteRoleAsync(int roleId, Guid tenantId)
        {
             var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId && r.TenantId == tenantId);
             if (role == null) return false;

             // Optional: Check usage before delete
             var inUse = await _context.UserCompanyAssignments.AnyAsync(uca => uca.RoleId == roleId);
             if (inUse) throw new InvalidOperationException("Cannot delete role as it is assigned to users.");

             _context.Roles.Remove(role);
             await _context.SaveChangesAsync();
             return true;
        }
    }
}
