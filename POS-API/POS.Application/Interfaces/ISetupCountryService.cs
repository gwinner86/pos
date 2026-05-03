using POS.Application.DTOs.Settings;

namespace POS.Application.Interfaces
{
    public interface ISetupCountryService
    {
        Task<IEnumerable<SetupCountryDto>> GetAllSetupCountriesAsync(Guid companyId);
        Task<SetupCountryDto?> GetSetupCountryByIdAsync(Guid id);
        Task<SetupCountryDto> CreateSetupCountryAsync(CreateSetupCountryDto request);
        Task<SetupCountryDto> UpdateSetupCountryAsync(Guid id, UpdateSetupCountryDto request);
        Task<bool> DeleteSetupCountryAsync(Guid id);
    }
}
