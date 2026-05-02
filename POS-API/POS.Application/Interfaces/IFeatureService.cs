using System.Collections.Generic;
using System.Threading.Tasks;
using POS.Application.DTOs.Features;

namespace POS.Application.Interfaces
{
    public interface IFeatureService
    {
        Task<IReadOnlyList<FeatureDto>> GetAllFeaturesAsync();
        Task<FeatureDto?> GetFeatureByIdAsync(int id);
        Task<FeatureDto> CreateFeatureAsync(CreateFeatureDto createFeatureDto);
        Task UpdateFeatureAsync(int id, UpdateFeatureDto updateFeatureDto);
        Task DeleteFeatureAsync(int id);
    }
}
