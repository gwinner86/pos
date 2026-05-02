using POS.Application.DTOs.Sale;

namespace POS.Application.Interfaces
{
    public interface ISaleService
    {
        Task<IEnumerable<SaleDto>> GetSalesAsync(Guid tenantId, Guid companyId, Guid? customerId = null, Guid? userId = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<SaleDto> GetSaleByIdAsync(Guid id, Guid tenantId);
        Task<SaleDto> CreateSaleAsync(CreateSaleDto dto, Guid tenantId, Guid userId, Guid companyId);
    }
}
