using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs.Tenant;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Infrastructure.Persistence;

namespace POS.Infrastructure.Services
{
    public class TenantService : ITenantService
    {
        private readonly ApplicationDbContext _context;

        public TenantService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TenantDto>> GetAllTenantsAsync()
        {
            var tenants = await _context.Tenants.AsNoTracking().ToListAsync();
            return tenants.Select(t => new TenantDto
            {
                Id = t.Id,
                TenantName = t.TenantName,
                FeatureId = t.FeatureId,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt
            });
        }

        public async Task<TenantDto?> GetTenantByNameAsync(string name)
        {
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.TenantName == name);
            if (tenant == null) return null;

            return new TenantDto
            {
                Id = tenant.Id,
                TenantName = tenant.TenantName,
                FeatureId = tenant.FeatureId,
                IsActive = tenant.IsActive,
                CreatedAt = tenant.CreatedAt
            };
        }

        public async Task<TenantDto?> GetTenantByIdAsync(Guid id)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null) return null;

            return new TenantDto
            {
                Id = tenant.Id,
                TenantName = tenant.TenantName,
                FeatureId = tenant.FeatureId,
                IsActive = tenant.IsActive,
                CreatedAt = tenant.CreatedAt
            };
        }

        public async Task<TenantDto> CreateTenantAsync(CreateTenantDto createTenantDto)
        {
            var tenant = new Tenant
            {
                TenantName = createTenantDto.TenantName,
                FeatureId = createTenantDto.FeatureId,
                IsActive = true
            };

            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            return new TenantDto
            {
                Id = tenant.Id,
                TenantName = tenant.TenantName,
                FeatureId = tenant.FeatureId,
                IsActive = tenant.IsActive,
                CreatedAt = tenant.CreatedAt
            };
        }

        public async Task UpdateTenantAsync(Guid id, UpdateTenantDto updateTenantDto)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null) throw new KeyNotFoundException($"Tenant with ID {id} not found.");

            tenant.TenantName = updateTenantDto.TenantName;
            tenant.FeatureId = updateTenantDto.FeatureId;
            tenant.IsActive = updateTenantDto.IsActive;
            tenant.UpdatedAt = DateTime.UtcNow;

            _context.Tenants.Update(tenant);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTenantAsync(Guid id)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null) throw new KeyNotFoundException($"Tenant with ID {id} not found.");

            // Soft delete or Hard delete? Usually Soft delete for Tenants.
            // But if requested CRUD, maybe hard delete? Assuming Soft delete via IsActive update or actual delete?
            // "IsActive" flag exists, but usually Delete implies removal or deactivation.
            // Let's do a hard delete if it's a CRUD request, or maybe just deactivation? 
            // Given "IsActive" is in Update, let's assume Delete might remove it or we can just leave it as is.
            // Let's implement standard remove for now, but usually for Tenants we don't delete.
            
            _context.Tenants.Remove(tenant);
            await _context.SaveChangesAsync();
        }
    }
}
