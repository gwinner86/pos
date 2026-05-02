using POS.Application.DTOs.Location;

namespace POS.Application.Interfaces
{
    public interface ILocationService
    {
        Task<IEnumerable<LocationResponse>> GetLocationsAsync(Guid tenantId);
        Task<IEnumerable<LocationResponse>> GetLocationsByCompanyIdAsync(Guid companyId, Guid tenantId);
        Task<LocationResponse?> GetLocationByIdAsync(Guid locationId, Guid tenantId);
        Task<Guid> CreateLocationAsync(CreateLocationDto request, Guid tenantId, Guid companyId);
        Task<bool> UpdateLocationAsync(Guid locationId, UpdateLocationDto request, Guid tenantId);
        Task<bool> DeleteLocationAsync(Guid locationId, Guid tenantId);
    }
}
