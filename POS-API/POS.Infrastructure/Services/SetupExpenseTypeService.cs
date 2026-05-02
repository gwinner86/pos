using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs.SetupExpenseType;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Infrastructure.Persistence;

namespace POS.Infrastructure.Services
{
    public class SetupExpenseTypeService : ISetupExpenseTypeService
    {
        private readonly ApplicationDbContext _context;

        public SetupExpenseTypeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SetupExpenseTypeDto> CreateAsync(CreateSetupExpenseTypeRequest request, Guid tenantId, Guid companyId)
        {
            var exists = await _context.SetupExpenseTypes
                .AnyAsync(e => e.TenantId == tenantId && e.CompanyId == companyId && e.Name == request.Name);

            if (exists)
                throw new ArgumentException("Expense Type with this name already exists.");

            var glAccount = await _context.GLAccounts.FindAsync(request.ExpenseAccountId);
            if (glAccount == null || glAccount.AccountType != "Expense")
                throw new ArgumentException("Invalid Expense Account.");

            var entity = new SetupExpenseType
            {
                TenantId = tenantId,
                CompanyId = companyId,
                Name = request.Name,
                Description = request.Description,
                ExpenseAccountId = request.ExpenseAccountId,
                IsActive = true
            };

            _context.SetupExpenseTypes.Add(entity);
            await _context.SaveChangesAsync();

            return MapToDto(entity, glAccount.AccountName);
        }

        public async Task<SetupExpenseTypeDto> UpdateAsync(Guid id, UpdateSetupExpenseTypeRequest request, Guid tenantId, Guid companyId)
        {
            var entity = await _context.SetupExpenseTypes
                .FirstOrDefaultAsync(e => e.Id == id && e.TenantId == tenantId && e.CompanyId == companyId);

            if (entity == null)
                throw new KeyNotFoundException("Expense Type not found.");

            var nameExists = await _context.SetupExpenseTypes
                .AnyAsync(e => e.TenantId == tenantId && e.CompanyId == companyId && e.Name == request.Name && e.Id != id);

            if (nameExists)
                throw new ArgumentException("Expense Type with this name already exists.");

            var glAccount = await _context.GLAccounts.FindAsync(request.ExpenseAccountId);
            if (glAccount == null || glAccount.AccountType != "Expense")
                throw new ArgumentException("Invalid Expense Account.");

            entity.Name = request.Name;
            entity.Description = request.Description;
            entity.ExpenseAccountId = request.ExpenseAccountId;
            entity.IsActive = request.IsActive;

            await _context.SaveChangesAsync();

            return MapToDto(entity, glAccount.AccountName);
        }

        public async Task<SetupExpenseTypeDto> GetByIdAsync(Guid id, Guid tenantId, Guid companyId)
        {
            var entity = await _context.SetupExpenseTypes
                .Include(e => e.ExpenseAccount)
                .FirstOrDefaultAsync(e => e.Id == id && e.TenantId == tenantId && e.CompanyId == companyId);

            if (entity == null)
                throw new KeyNotFoundException("Expense Type not found.");

            return MapToDto(entity, entity.ExpenseAccount?.AccountName ?? "Unknown");
        }

        public async Task<IEnumerable<SetupExpenseTypeDto>> GetAllAsync(Guid tenantId, Guid companyId, bool includeInactive = false)
        {
            var query = _context.SetupExpenseTypes
                .Include(e => e.ExpenseAccount)
                .Where(e => e.TenantId == tenantId && e.CompanyId == companyId);

            if (!includeInactive)
                query = query.Where(e => e.IsActive);

            var list = await query.ToListAsync();

            return list.Select(e => MapToDto(e, e.ExpenseAccount?.AccountName ?? "Unknown"));
        }

        public async Task DeleteAsync(Guid id, Guid tenantId, Guid companyId)
        {
            var entity = await _context.SetupExpenseTypes
                .FirstOrDefaultAsync(e => e.Id == id && e.TenantId == tenantId && e.CompanyId == companyId);

            if (entity == null)
                throw new KeyNotFoundException("Expense Type not found.");

            _context.SetupExpenseTypes.Remove(entity);
            await _context.SaveChangesAsync();
        }

        private static SetupExpenseTypeDto MapToDto(SetupExpenseType entity, string accountName)
        {
            return new SetupExpenseTypeDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                ExpenseAccountId = entity.ExpenseAccountId,
                ExpenseAccountName = accountName,
                IsActive = entity.IsActive
            };
        }
    }
}
