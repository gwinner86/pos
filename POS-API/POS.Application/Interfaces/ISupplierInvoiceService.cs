using POS.Application.DTOs.SupplierInvoice;

namespace POS.Application.Interfaces
{
    public interface ISupplierInvoiceService
    {
        Task<IEnumerable<SupplierInvoiceDto>> GetInvoicesAsync(Guid tenantId, Guid companyId);
        Task<SupplierInvoiceDto> GetInvoiceByIdAsync(Guid id, Guid tenantId);
        Task<SupplierInvoiceDto> CreateInvoiceAsync(CreateSupplierInvoiceDto dto, Guid tenantId, Guid userId, Guid companyId);
        Task<SupplierInvoiceDto> UpdateInvoiceAsync(Guid id, UpdateSupplierInvoiceDto dto, Guid tenantId, Guid userId);
    }
}
