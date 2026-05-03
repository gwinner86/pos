using POS.Application.DTOs.Category;

namespace POS.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(Guid tenantId);
        Task<CategoryDto> GetCategoryByIdAsync(Guid id);
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto, Guid tenantId, Guid userId, Guid companyId); // Added CompanyId
        Task<CategoryDto> UpdateCategoryAsync(Guid id, UpdateCategoryDto updateCategoryDto, Guid userId);
        Task DeleteCategoryAsync(Guid id);
    }
}
