using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using POS.Application.DTOs.Settings;

namespace POS.Application.Interfaces
{
    public interface ISetupVATService
    {
        Task<IEnumerable<SetupVATDto>> GetAllAsync(Guid tenantId, Guid companyId);
        Task<SetupVATDto> GetByIdAsync(Guid id, Guid tenantId);
        Task<SetupVATDto> CreateAsync(CreateSetupVATDto dto, Guid tenantId, Guid userId);
        Task<SetupVATDto> UpdateAsync(Guid id, UpdateSetupVATDto dto, Guid tenantId, Guid userId);
        Task DeleteAsync(Guid id, Guid tenantId);
    }
}
