using POS.Application.DTOs.Supplier;

namespace POS.Application.Interfaces
{
    public interface ISupplierService
    {
        Task<IEnumerable<SupplierDto>> GetSuppliersAsync(Guid tenantId, Guid companyId, Guid? locationId = null);
        Task<IEnumerable<SupplierDto>> GetInactiveSuppliersAsync(Guid tenantId, Guid companyId);
        Task<SupplierDto> GetSupplierByIdAsync(Guid id, Guid tenantId);
        Task<SupplierDto> CreateSupplierAsync(CreateSupplierDto dto, Guid tenantId, Guid userId, Guid companyId);
        Task<SupplierDto> UpdateSupplierAsync(Guid id, UpdateSupplierDto dto, Guid tenantId, Guid userId);
        Task<bool> DeleteSupplierAsync(Guid id, Guid tenantId);
    }
}
