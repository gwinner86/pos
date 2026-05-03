using POS.Application.DTOs.SetupExpenseType;

namespace POS.Application.Interfaces
{
    public interface ISetupExpenseTypeService
    {
        Task<SetupExpenseTypeDto> CreateAsync(CreateSetupExpenseTypeRequest request, Guid tenantId, Guid companyId);
        Task<SetupExpenseTypeDto> UpdateAsync(Guid id, UpdateSetupExpenseTypeRequest request, Guid tenantId, Guid companyId);
        Task<SetupExpenseTypeDto> GetByIdAsync(Guid id, Guid tenantId, Guid companyId);
        Task<IEnumerable<SetupExpenseTypeDto>> GetAllAsync(Guid tenantId, Guid companyId, bool includeInactive = false);
        Task DeleteAsync(Guid id, Guid tenantId, Guid companyId);
    }
}
