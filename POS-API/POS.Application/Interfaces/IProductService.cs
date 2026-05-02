using POS.Application.DTOs.Product;

namespace POS.Application.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetProductsAsync(Guid tenantId, Guid? locationId = null);
        Task<ProductDto> GetProductByIdAsync(Guid productId, Guid tenantId, Guid? locationId = null);
        Task<ProductDto> CreateProductAsync(CreateProductDto request, Guid tenantId, Guid userId, Guid companyId);
        Task<ProductDto> UpdateProductAsync(Guid productId, UpdateProductDto request, Guid tenantId, Guid userId);
        Task<bool> DeleteProductAsync(Guid productId, Guid tenantId, Guid userId);
        Task<int> DeleteAllProductsAsync(Guid tenantId, Guid userId);
        Task<int> BulkUploadProductsAsync(Stream fileStream, string fileName, Guid tenantId, Guid userId, Guid companyId);
        Task<IEnumerable<Guid>> GetProductLocationAssignmentsAsync(Guid productId, Guid tenantId);
        Task UpdateProductLocationAssignmentsAsync(Guid productId, List<Guid> locationIds, Guid tenantId, Guid userId, Guid companyId);
    }
}
