using POS.Application.DTOs.Pricing;

namespace POS.Application.Interfaces
{
    public interface IPricingService
    {
        Task<IEnumerable<PricingDto>> GetPricingsAsync(Guid tenantId, Guid companyId);
        Task<PricingDto> GetPricingByIdAsync(Guid id, Guid tenantId);
        Task<PricingDto> CreatePricingAsync(CreatePricingDto dto, Guid tenantId, Guid userId, Guid companyId);
        Task<PricingDto> UpdatePricingAsync(Guid id, UpdatePricingDto dto, Guid tenantId, Guid userId);
        Task<bool> DeletePricingAsync(Guid id, Guid tenantId, Guid userId, string reason = "Soft Delete");
    }
}
