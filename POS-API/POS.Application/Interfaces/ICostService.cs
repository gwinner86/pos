using POS.Application.DTOs.Cost;

namespace POS.Application.Interfaces
{
    public interface ICostService
    {
        Task<IEnumerable<CostDto>> GetCostsAsync(Guid tenantId, Guid companyId);
        Task<CostDto> GetCostByIdAsync(Guid id, Guid tenantId);
        Task<CostDto> CreateCostAsync(CreateCostDto dto, Guid tenantId, Guid userId, Guid companyId);
        Task<CostDto> UpdateCostAsync(Guid id, UpdateCostDto dto, Guid tenantId, Guid userId);
        Task<bool> DeleteCostAsync(Guid id, Guid tenantId);
    }
}
