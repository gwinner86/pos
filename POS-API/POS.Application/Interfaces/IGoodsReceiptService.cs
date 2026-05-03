using POS.Application.DTOs.GoodsReceipt;

namespace POS.Application.Interfaces
{
    public interface IGoodsReceiptService
    {
        Task<IEnumerable<GoodsReceiptDto>> GetGoodsReceiptsAsync(Guid tenantId, Guid companyId);
        Task<GoodsReceiptDto> GetGoodsReceiptByIdAsync(Guid id, Guid tenantId);
        Task<GoodsReceiptDto> CreateGoodsReceiptAsync(CreateGoodsReceiptDto dto, Guid tenantId, Guid userId, Guid companyId);
        Task<GoodsReceiptDto> UpdateGoodsReceiptAsync(Guid id, UpdateGoodsReceiptDto dto, Guid tenantId, Guid userId);
        Task<GoodsReceiptDto> ApproveGoodsReceiptAsync(Guid id, Guid tenantId, Guid userId, Guid companyId);
    }
}
