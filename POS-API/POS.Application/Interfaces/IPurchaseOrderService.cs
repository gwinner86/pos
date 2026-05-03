using POS.Application.DTOs.PurchaseOrder;

namespace POS.Application.Interfaces
{
    public interface IPurchaseOrderService
    {
        Task<IEnumerable<PurchaseOrderDto>> GetPurchaseOrdersAsync(Guid tenantId, Guid companyId);
        Task<PurchaseOrderDto> GetPurchaseOrderByIdAsync(Guid id, Guid tenantId);
        Task<PurchaseOrderDto> CreatePurchaseOrderAsync(CreatePurchaseOrderDto dto, Guid tenantId, Guid userId, Guid companyId);
        Task<PurchaseOrderDto> UpdatePurchaseOrderAsync(Guid id, UpdatePurchaseOrderDto dto, Guid tenantId, Guid userId);
        Task<PurchaseOrderDto> ReceivePurchaseOrderAsync(Guid id, Guid tenantId, Guid userId);
        Task<PurchaseOrderDto> InstantPurchaseAsync(CreatePurchaseOrderDto dto, Guid tenantId, Guid userId, Guid companyId);
    }
}
