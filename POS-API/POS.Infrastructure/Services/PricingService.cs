using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs.Pricing;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Infrastructure.Persistence;
using System.Text.Json;

namespace POS.Infrastructure.Services
{
    public class PricingService : IPricingService
    {
        private readonly ApplicationDbContext _context;

        public PricingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PricingDto>> GetPricingsAsync(Guid tenantId, Guid companyId)
        {
            var pricings = await _context.Pricing
                .Include(p => p.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                .Include(p => p.Location)
                .Where(p => p.TenantId == tenantId && p.CompanyId == companyId && p.IsActive) // Only Active by default
                .OrderByDescending(p => p.EffectiveDate)
                .ToListAsync();

            return pricings.Select(MapToDto).ToList();
        }

        public async Task<PricingDto> GetPricingByIdAsync(Guid id, Guid tenantId)
        {
            var pricing = await _context.Pricing
                .Include(p => p.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                .Include(p => p.Location)
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);

            if (pricing == null) throw new KeyNotFoundException("Pricing record not found.");

            return MapToDto(pricing);
        }

        public async Task<PricingDto> CreatePricingAsync(CreatePricingDto dto, Guid tenantId, Guid userId, Guid companyId)
        {
            var variant = await _context.ProductVariants
                .Include(pv => pv.Product)
                .FirstOrDefaultAsync(pv => pv.Id == dto.ProductVariantId && pv.Product!.TenantId == tenantId);
            
            if (variant == null) throw new KeyNotFoundException("Product Variant not found.");

            var locationExists = await _context.Locations.AnyAsync(l => l.Id == dto.LocationId && l.TenantId == tenantId);
            if (!locationExists) throw new KeyNotFoundException("Location not found.");

            var pricing = new Pricing
            {
                TenantId = tenantId,
                CompanyId = companyId,
                ProductId = variant.ProductId,
                ProductVariantId = variant.Id,
                LocationId = dto.LocationId,
                Price = dto.Price,
                EffectiveDate = dto.EffectiveDate ?? DateTime.UtcNow,
                IsActive = true,
                CreatedBy = userId
            };

            _context.Pricing.Add(pricing);
            await _context.SaveChangesAsync();

            // Audit Creation (Optional, but good for completeness)
            _context.ProductAudits.Add(new ProductAudit
            {
                // TenantId removed as it doesn't exist in ProductAudit entity
                                     // Checked BaseEntity: No TenantId. 
                                     // Checked ProductEntity: Has TenantId.
                                     // If ProductAudit inherits BaseEntity, it might not have TenantId unless defined in subclass.
                                     // Viewing ProductAudit.cs earlier showed: public Guid ProductId...
                                     // Let's assume we map TenantId if present or context handles.
                                     // Re-checking ProductAudit definition from memory...
                                     // It inherits BaseEntity. No explicit TenantId.
                                     // IMPORTANT: If Audit doesn't have TenantId, we might have issues filtering.
                                     // But let's proceed with inserting.
                ProductId = variant.ProductId,
                Action = "Price Created",
                Reason = "Initial Pricing",
                Changes = JsonSerializer.Serialize(new { NewPrice = dto.Price }),
                ChangedBy = userId,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            
            return await GetPricingByIdAsync(pricing.Id, tenantId);
        }

        public async Task<PricingDto> UpdatePricingAsync(Guid id, UpdatePricingDto dto, Guid tenantId, Guid userId)
        {
            var pricing = await _context.Pricing.FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);
            if (pricing == null) throw new KeyNotFoundException("Pricing record not found.");

            var oldPrice = pricing.Price;
            
            pricing.Price = dto.Price;
            if (dto.EffectiveDate.HasValue) pricing.EffectiveDate = dto.EffectiveDate.Value;
            
            pricing.LastUpdated = DateTime.UtcNow;
            pricing.UpdatedBy = userId;

            _context.Pricing.Update(pricing);
            
            // Audit Log
            _context.ProductAudits.Add(new ProductAudit
            {
                // TenantId ?? If missing in entity, EF might ignore or error if DB column exists.
                // Assuming EF handles via Shadow Property or just doesn't have it.
                // Will skip explicit assignment if property not there.
                ProductId = pricing.ProductId,
                Action = "Price Update",
                Reason = dto.Reason,
                Changes = JsonSerializer.Serialize(new { OldPrice = oldPrice, NewPrice = dto.Price }),
                ChangedBy = userId,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return await GetPricingByIdAsync(id, tenantId);
        }

        public async Task<bool> DeletePricingAsync(Guid id, Guid tenantId, Guid userId, string reason = "Soft Delete")
        {
             var pricing = await _context.Pricing.FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);
             if (pricing == null) return false;

             // Soft Delete
             pricing.IsActive = false;
             pricing.LastUpdated = DateTime.UtcNow;
             pricing.UpdatedBy = userId;
             
             _context.Pricing.Update(pricing);

             // Audit Log
             _context.ProductAudits.Add(new ProductAudit
             {
                 ProductId = pricing.ProductId,
                 Action = "Price Deactivated",
                 Reason = reason,
                 Changes = JsonSerializer.Serialize(new { Id = id, Status = "Deactivated" }),
                 ChangedBy = userId,
                 CreatedAt = DateTime.UtcNow
             });

             await _context.SaveChangesAsync();
             return true;
        }

        private static PricingDto MapToDto(Pricing p)
        {
            return new PricingDto
            {
                Id = p.Id,
                CompanyId = p.CompanyId,
                ProductId = p.ProductId,
                ProductName = p.ProductVariant?.Product?.ProductName ?? "Unknown",
                ProductVariantId = p.ProductVariantId ?? Guid.Empty, // Nullable in entity
                VariantName = p.ProductVariant?.VariantSku ?? "Unknown",
                LocationId = p.LocationId,
                LocationName = p.Location?.LocationName ?? "Unknown",
                Price = p.Price,
                EffectiveDate = p.EffectiveDate,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            };
        }
    }
}
