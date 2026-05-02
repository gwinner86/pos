using POS.Application.DTOs.Customer;

namespace POS.Application.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerDto>> GetCustomersAsync(Guid tenantId, Guid companyId);
        Task<CustomerDto> GetCustomerByIdAsync(Guid id, Guid tenantId);
        Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto, Guid tenantId, Guid userId, Guid companyId);
        Task<CustomerDto> UpdateCustomerAsync(Guid id, UpdateCustomerDto dto, Guid tenantId, Guid userId);
        Task<bool> DeleteCustomerAsync(Guid id, Guid tenantId);
    }
}
