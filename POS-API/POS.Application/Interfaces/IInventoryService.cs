using POS.Application.DTOs.Inventory;

namespace POS.Application.Interfaces
{
    public interface IInventoryService
    {
        Task<IEnumerable<InventoryDto>> GetAllInventoriesAsync(Guid tenantId);
        Task<InventoryDto> GetInventoryAsync(Guid productVariantId, Guid locationId, Guid tenantId);
        Task<InventoryDto> GetInventoryByIdAsync(Guid inventoryId, Guid tenantId);
        Task<InventoryDto> CreateInventoryAsync(CreateInventoryDto request, Guid tenantId, Guid userId, Guid companyId);
        Task<InventoryDto> AdjustInventoryAsync(AdjustInventoryDto request, Guid tenantId, Guid userId, Guid companyId);
    }
}
