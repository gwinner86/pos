using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs.Customer;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Infrastructure.Persistence;

namespace POS.Infrastructure.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _context;

        public CustomerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CustomerDto>> GetCustomersAsync(Guid tenantId, Guid companyId)
        {
            var customers = await _context.Customers
                .Where(c => c.TenantId == tenantId && c.CompanyId == companyId && c.IsActive)
                .OrderBy(c => c.FirstName)
                .ToListAsync();

            return customers.Select(MapToDto).ToList();
        }

        public async Task<CustomerDto> GetCustomerByIdAsync(Guid id, Guid tenantId)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);

            if (customer == null) throw new KeyNotFoundException("Customer not found.");

            return MapToDto(customer);
        }

        public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto, Guid tenantId, Guid userId, Guid companyId)
        {
            // Optional: Duplicate Check on Code or Email
            if (await _context.Customers.AnyAsync(c => c.CustomerCode == dto.CustomerCode && c.TenantId == tenantId))
            {
                throw new InvalidOperationException("Customer Code already exists.");
            }

            var customer = new Customer
            {
                TenantId = tenantId,
                CompanyId = companyId,
                CustomerCode = dto.CustomerCode,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                CompanyName = dto.CompanyName,
                Email = dto.Email,
                Phone = dto.Phone,
                AddressLine1 = dto.AddressLine1,
                City = dto.City,
                CreatedBy = userId,
                IsActive = true
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return MapToDto(customer);
        }

        public async Task<CustomerDto> UpdateCustomerAsync(Guid id, UpdateCustomerDto dto, Guid tenantId, Guid userId)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);
            if (customer == null) throw new KeyNotFoundException("Customer not found.");

            if (dto.FirstName != null) customer.FirstName = dto.FirstName;
            if (dto.LastName != null) customer.LastName = dto.LastName;
            if (dto.CompanyName != null) customer.CompanyName = dto.CompanyName;
            if (dto.Email != null) customer.Email = dto.Email;
            if (dto.Phone != null) customer.Phone = dto.Phone;
            if (dto.AddressLine1 != null) customer.AddressLine1 = dto.AddressLine1;
            if (dto.City != null) customer.City = dto.City;

            // UpdatedBy usually tracked in BaseEntity if present, but current Customer.cs showed CreatedBy only?
            // BaseEntity typically has UpdatedAt.
            // If explicit UpdatedBy needed, check entity again. Assuming framework handles or not explicitly required by previous patterns.

            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            return MapToDto(customer);
        }

        public async Task<bool> DeleteCustomerAsync(Guid id, Guid tenantId)
        {
             var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);
             if (customer == null) return false;

             // Soft Delete
             customer.IsActive = false;
             _context.Customers.Update(customer);
             await _context.SaveChangesAsync();
             return true;
        }

        private static CustomerDto MapToDto(Customer c)
        {
            return new CustomerDto
            {
                Id = c.Id,
                CompanyId = c.CompanyId,
                CustomerCode = c.CustomerCode,
                FirstName = c.FirstName,
                LastName = c.LastName,
                CompanyName = c.CompanyName,
                Email = c.Email,
                Phone = c.Phone,
                AddressLine1 = c.AddressLine1,
                City = c.City,
                LoyaltyPoints = c.LoyaltyPoints,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            };
        }
    }
}
