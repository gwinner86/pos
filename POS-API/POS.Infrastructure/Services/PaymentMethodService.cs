using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs.PaymentMethod;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Infrastructure.Persistence;

namespace POS.Infrastructure.Services
{
    public class PaymentMethodService : IPaymentMethodService
    {
        private readonly ApplicationDbContext _context;

        public PaymentMethodService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentMethodDto> CreatePaymentMethodAsync(CreatePaymentMethodDto dto, Guid userId)
        {
            var companyId = await GetCompanyIdForUser(userId);

            var method = new PaymentMethod
            {
                CompanyId = companyId,
                MethodName = dto.MethodName,
                PaymentType = dto.PaymentType,
                IsActive = dto.IsActive,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.PaymentMethods.Add(method);
            await _context.SaveChangesAsync();

            return MapToDto(method);
        }

        public async Task<PaymentMethodDto> UpdatePaymentMethodAsync(UpdatePaymentMethodDto dto, Guid userId)
        {
            var companyId = await GetCompanyIdForUser(userId);
            
            var method = await _context.PaymentMethods
                .FirstOrDefaultAsync(pm => pm.PaymentMethodId == dto.PaymentMethodId && pm.CompanyId == companyId);

            if (method == null)
                throw new KeyNotFoundException($"Payment Method with ID {dto.PaymentMethodId} not found.");

            method.MethodName = dto.MethodName;
            method.PaymentType = dto.PaymentType;
            method.IsActive = dto.IsActive;
            
            _context.PaymentMethods.Update(method);
            await _context.SaveChangesAsync();

            return MapToDto(method);
        }

        public async Task<List<PaymentMethodDto>> GetAllPaymentMethodsAsync()
        {
            // Deprecated/Unsafe - better to use ForUser
             return await _context.PaymentMethods
                .Select(pm => MapToDto(pm))
                .ToListAsync();
        }

        public async Task<List<PaymentMethodDto>> GetAllPaymentMethodsForUserAsync(Guid userId)
        {
             var companyId = await GetCompanyIdForUser(userId);
             return await _context.PaymentMethods
                .Where(pm => pm.CompanyId == companyId)
                .Select(pm => MapToDto(pm))
                .ToListAsync();
        }

        private async Task<Guid> GetCompanyIdForUser(Guid userId)
        {
            // Assuming UserCompanyAssignment holds the primary company. 
            // If multiple, taking first or forcing context. For now, taking first.
            var assignment = await _context.UserCompanyAssignments
                .FirstOrDefaultAsync(uca => uca.UserId == userId);
            
            if (assignment == null)
                throw new UnauthorizedAccessException("User is not assigned to any company.");
                
            return assignment.CompanyId;
        }

        public async Task<PaymentMethodDto> GetPaymentMethodByIdAsync(int id)
        {
            var method = await _context.PaymentMethods.FindAsync(id);
            if (method == null) throw new KeyNotFoundException("Payment Method not found.");
            return MapToDto(method);
        }

        public async Task<bool> DeletePaymentMethodAsync(int id)
        {
            var method = await _context.PaymentMethods.FindAsync(id);
            if (method == null) throw new KeyNotFoundException("Payment Method not found.");

            _context.PaymentMethods.Remove(method);
            await _context.SaveChangesAsync();
            return true;
        }

        private static PaymentMethodDto MapToDto(PaymentMethod pm)
        {
            return new PaymentMethodDto
            {
                PaymentMethodId = pm.PaymentMethodId,
                MethodName = pm.MethodName,
                PaymentType = pm.PaymentType,
                IsActive = pm.IsActive,
                CreatedAt = pm.CreatedAt
            };
        }
    }
}
