using POS.Application.DTOs.Tenant;

namespace POS.Application.Interfaces
{
    public interface ITenantService
    {
        Task<IEnumerable<TenantDto>> GetAllTenantsAsync();
        Task<TenantDto?> GetTenantByIdAsync(Guid id);
        Task<TenantDto?> GetTenantByNameAsync(string name);
        Task<TenantDto> CreateTenantAsync(CreateTenantDto createTenantDto);
        Task UpdateTenantAsync(Guid id, UpdateTenantDto updateTenantDto);
        Task DeleteTenantAsync(Guid id);
    }
}
