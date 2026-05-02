using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs.Supplier;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Infrastructure.Persistence;

namespace POS.Infrastructure.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly ApplicationDbContext _context;

        public SupplierService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SupplierDto>> GetSuppliersAsync(Guid tenantId, Guid companyId, Guid? locationId = null)
        {
            var query = _context.Suppliers
                .Include(s => s.Location)
                .Where(s => s.TenantId == tenantId && s.CompanyId == companyId && s.IsActive);

            if (locationId.HasValue)
            {
                // Show suppliers for this location OR global suppliers (null)
                // If user wants STRICT isolation, remove "|| s.LocationId == null"
                // Assuming they want to see "Available" suppliers
                query = query.Where(s => s.LocationId == locationId.Value || s.LocationId == null);
            }

            var suppliers = await query.ToListAsync();

            return suppliers.Select(MapToDto).ToList();
        }

        public async Task<IEnumerable<SupplierDto>> GetInactiveSuppliersAsync(Guid tenantId, Guid companyId)
        {
            var suppliers = await _context.Suppliers
                .Include(s => s.Location)
                .Where(s => s.TenantId == tenantId && s.CompanyId == companyId && !s.IsActive)
                .ToListAsync();

            return suppliers.Select(MapToDto).ToList();
        }

        public async Task<SupplierDto> GetSupplierByIdAsync(Guid id, Guid tenantId)
        {
            var supplier = await _context.Suppliers
                .Include(s => s.Location)
                .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId);

            if (supplier == null) throw new KeyNotFoundException("Supplier not found.");

            return MapToDto(supplier);
        }

        public async Task<SupplierDto> CreateSupplierAsync(CreateSupplierDto dto, Guid tenantId, Guid userId, Guid companyId)
        {
            // Check uniqueness (Global or scoped to Location?)
            // Assuming uniqueness per company is still main check, but location-specific could be same name? 
            // For safety, let's keep Company-wide unique name for now to avoid confusion.
            var exists = await _context.Suppliers
                .AnyAsync(s => s.SupplierName == dto.SupplierName && s.CompanyId == companyId);
            
            if (exists) throw new InvalidOperationException($"Supplier '{dto.SupplierName}' already exists.");

            var supplier = new Supplier
            {
                TenantId = tenantId,
                CompanyId = companyId,
                SupplierName = dto.SupplierName,
                ContactName = dto.ContactName,
                ContactEmail = dto.ContactEmail,
                Phone = dto.Phone,
                Terms = dto.Terms,
                IsActive = dto.IsActive,
                LocationId = dto.LocationId, // Mapping
                CreatedBy = userId
            };

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            // Reload to get Location name if needed
            if (supplier.LocationId.HasValue) 
            {
                await _context.Entry(supplier).Reference(s => s.Location).LoadAsync();
            }

            return MapToDto(supplier);
        }

        public async Task<SupplierDto> UpdateSupplierAsync(Guid id, UpdateSupplierDto dto, Guid tenantId, Guid userId)
        {
             var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId);

            if (supplier == null) throw new KeyNotFoundException("Supplier not found.");

            if (supplier.SupplierName != dto.SupplierName)
            {
                var exists = await _context.Suppliers
                    .AnyAsync(s => s.SupplierName == dto.SupplierName && s.CompanyId == supplier.CompanyId && s.Id != id);
                if (exists) throw new InvalidOperationException($"Supplier '{dto.SupplierName}' already exists.");
            }

            supplier.SupplierName = dto.SupplierName;
            supplier.ContactName = dto.ContactName;
            supplier.ContactEmail = dto.ContactEmail;
            supplier.Phone = dto.Phone;
            supplier.Terms = dto.Terms;
            supplier.IsActive = dto.IsActive;
            supplier.LocationId = dto.LocationId; // Update Location
            // supplier.UpdatedBy = userId; 
            
            _context.Suppliers.Update(supplier);
            await _context.SaveChangesAsync();

            if (supplier.LocationId.HasValue && supplier.Location == null)
            {
                await _context.Entry(supplier).Reference(s => s.Location).LoadAsync();
            }

            return MapToDto(supplier);
        }

        public async Task<bool> DeleteSupplierAsync(Guid id, Guid tenantId)
        {
             var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId);

            if (supplier == null) throw new KeyNotFoundException("Supplier not found.");
            
            // Check for dependencies
            var hasDependencies = await _context.SupplierInvoices.AnyAsync(si => si.SupplierId == id) ||
                                  await _context.PurchaseOrders.AnyAsync(po => po.SupplierId == id) ||
                                  await _context.GoodsReceipts.AnyAsync(gr => gr.SupplierId == id);

            if (hasDependencies)
            {
                // Soft Delete
                supplier.IsActive = false;
                _context.Suppliers.Update(supplier);
                await _context.SaveChangesAsync();
                return false; // Soft deleted
            }
            else
            {
                // Hard Delete
                _context.Suppliers.Remove(supplier);
                await _context.SaveChangesAsync();
                return true; // Hard deleted
            }
        }

        private static SupplierDto MapToDto(Supplier s)
        {
            return new SupplierDto
            {
                Id = s.Id,
                CompanyId = s.CompanyId,
                SupplierName = s.SupplierName,
                ContactName = s.ContactName,
                ContactEmail = s.ContactEmail,
                Phone = s.Phone,
                Terms = s.Terms,
                IsActive = s.IsActive,
                LocationId = s.LocationId,
                LocationName = s.Location?.LocationName,
                //  CreatedAt = s.CreatedAt 
            };
        }
    }
}
