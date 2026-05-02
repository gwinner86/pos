using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs.Cost;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Infrastructure.Persistence;

namespace POS.Infrastructure.Services
{
    public class CostService : ICostService
    {
        private readonly ApplicationDbContext _context;

        public CostService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CostDto>> GetCostsAsync(Guid tenantId, Guid companyId)
        {
            var costs = await _context.Costs
                .Include(c => c.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                .Include(c => c.Supplier)
                .Include(c => c.Location)
                .Where(c => c.TenantId == tenantId && c.CompanyId == companyId)
                .OrderByDescending(c => c.EffectiveDate)
                .ToListAsync();

            return costs.Select(MapToDto).ToList();
        }

        public async Task<CostDto> GetCostByIdAsync(Guid id, Guid tenantId)
        {
            var cost = await _context.Costs
                .Include(c => c.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                .Include(c => c.Supplier)
                .Include(c => c.Location)
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);

            if (cost == null) throw new KeyNotFoundException("Cost record not found.");

            return MapToDto(cost);
        }

        public async Task<CostDto> CreateCostAsync(CreateCostDto dto, Guid tenantId, Guid userId, Guid companyId)
        {
            // Validate Logic using Transaction not strictly needed for single entity but good practice? 
            // Stick to standard logic for now.

            // 1. Validate Entities
            var variant = await _context.ProductVariants
                .Include(pv => pv.Product)
                .FirstOrDefaultAsync(pv => pv.Id == dto.ProductVariantId && pv.Product!.TenantId == tenantId);
            
            if (variant == null) throw new KeyNotFoundException("Product Variant not found.");

            var supplierExists = await _context.Suppliers.AnyAsync(s => s.Id == dto.SupplierId && s.TenantId == tenantId);
            if (!supplierExists) throw new KeyNotFoundException("Supplier not found.");

            if (dto.LocationId.HasValue)
            {
                var locationExists = await _context.Locations.AnyAsync(l => l.Id == dto.LocationId.Value && l.TenantId == tenantId);
                if (!locationExists) throw new KeyNotFoundException("Location not found.");
            }

            var cost = new Cost
            {
                TenantId = tenantId,
                CompanyId = companyId,
                ProductId = variant.ProductId,
                ProductVariantId = variant.Id,
                SupplierId = dto.SupplierId,
                LocationId = dto.LocationId,
                CostValue = dto.CostValue,
                EffectiveDate = dto.EffectiveDate ?? DateTime.UtcNow,
                CreatedBy = userId
            };

            _context.Costs.Add(cost);
            await _context.SaveChangesAsync();

            // Reload to get names included
            return await GetCostByIdAsync(cost.Id, tenantId);
        }

        public async Task<CostDto> UpdateCostAsync(Guid id, UpdateCostDto dto, Guid tenantId, Guid userId)
        {
            var cost = await _context.Costs.FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);
            if (cost == null) throw new KeyNotFoundException("Cost record not found.");

            if (dto.CostValue.HasValue) cost.CostValue = dto.CostValue.Value;
            if (dto.EffectiveDate.HasValue) cost.EffectiveDate = dto.EffectiveDate.Value;

            // Optional: Track UpdatedBy/Time? BaseEntity supports it usually but Cost entity def showed CreatedBy only?
            // Checked entity: Doesn't have UpdatedBy in explicit properties shown in file view, but inherits BaseEntity?
            // BaseEntity usually has CreatedAt/By only? 
            // Re-checking file view... Cost.cs snippet showed:
            // public Guid CreatedBy { get; set; }
            // Inherits BaseEntity (Id, IsDeleted, CreatedAt, UpdatedAt?)
            // Let's assume standard BaseEntity has UpdatedAt.
            // But if not, we define what is there.
            
            _context.Costs.Update(cost);
            await _context.SaveChangesAsync();

            return await GetCostByIdAsync(id, tenantId);
        }

        public async Task<bool> DeleteCostAsync(Guid id, Guid tenantId)
        {
             var cost = await _context.Costs.FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);
             if (cost == null) return false;

             _context.Costs.Remove(cost);
             await _context.SaveChangesAsync();
             return true;
        }

        private static CostDto MapToDto(Cost c)
        {
            return new CostDto
            {
                Id = c.Id,
                CompanyId = c.CompanyId,
                ProductId = c.ProductId,
                ProductName = c.ProductVariant?.Product?.ProductName ?? "Unknown",
                ProductVariantId = c.ProductVariantId,
                VariantName = c.ProductVariant?.VariantSku ?? "Unknown",
                SupplierId = c.SupplierId,
                SupplierName = c.Supplier?.SupplierName ?? "Unknown",
                LocationId = c.LocationId,
                LocationName = c.Location?.LocationName ?? "All Locations",
                CostValue = c.CostValue,
                EffectiveDate = c.EffectiveDate,
                CreatedAt = c.CreatedAt
            };
        }
    }
}
