using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs.Permission;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Infrastructure.Persistence;

namespace POS.Infrastructure.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly ApplicationDbContext _context;

        public PermissionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync()
        {
            return await _context.Permissions
                .OrderBy(p => p.Group).ThenBy(p => p.Name)
                .Select(p => new PermissionDto
                {
                    PermissionId = p.PermissionId,
                    Name = p.Name,
                    Description = p.Description,
                    Group = p.Group
                })
                .ToListAsync();
        }

        public async Task AssignPermissionsToRoleAsync(int roleId, List<int> permissionIds)
        {
            var role = await _context.Roles
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.RoleId == roleId);

            if (role == null)
                throw new KeyNotFoundException($"Role with ID {roleId} not found.");

            // Clear existing permissions
            _context.RolePermissions.RemoveRange(role.RolePermissions);
            
            // Validate Permission IDs exist
            var validPermissions = await _context.Permissions
                .Where(p => permissionIds.Contains(p.PermissionId))
                .Select(p => p.PermissionId)
                .ToListAsync();

            if (validPermissions.Count != permissionIds.Count)
            {
                // Optional: Throw if some IDs are invalid, or just ignore them. 
                // Stick to safe side:
                // throw new ArgumentException("One or more permission IDs are invalid.");
                // For now, let's just add the valid ones.
            }

            foreach (var permId in validPermissions)
            {
                _context.RolePermissions.Add(new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permId
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<PermissionDto>> GetPermissionsByRoleIdAsync(int roleId)
        {
             // Verify role exists locally or assume caller knows (But better to check if strictly needed, 
             // here we can just join)
             
            return await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Include(rp => rp.Permission)
                .Select(rp => new PermissionDto
                {
                    PermissionId = rp.Permission.PermissionId,
                    Name = rp.Permission.Name,
                    Description = rp.Permission.Description,
                    Group = rp.Permission.Group
                })
                .ToListAsync();
        }
    }
}
