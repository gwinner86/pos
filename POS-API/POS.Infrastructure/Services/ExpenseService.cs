using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs.Expense;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Infrastructure.Persistence;

namespace POS.Infrastructure.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAccountingService _accountingService;

        public ExpenseService(ApplicationDbContext context, IAccountingService accountingService)
        {
            _context = context;
            _accountingService = accountingService;
        }

        public async Task<ExpenseDto> CreateExpenseAsync(CreateExpenseDto dto, Guid userId, Guid tenantId, Guid companyId)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Basic Validation
                    var expenseType = await _context.SetupExpenseTypes
                        .Include(t => t.ExpenseAccount)
                        .FirstOrDefaultAsync(t => t.Id == dto.ExpenseTypeId);
                    if (expenseType == null || expenseType.ExpenseAccount == null) throw new ArgumentException("Invalid Expense Type or Account.");
                    
                    var paymentAccount = await _context.GLAccounts.FindAsync(dto.PaymentAccountId);
                    if (paymentAccount == null) throw new ArgumentException("Invalid Payment Account.");

                    // Create Expense Record
                    var expense = new Expense
                    {
                        TenantId = tenantId,
                        CompanyId = companyId,
                        LocationId = dto.LocationId,
                        ExpenseDate = dto.ExpenseDate,
                        Description = dto.Description,
                        ReferenceNumber = dto.ReferenceNumber,
                        Amount = dto.Amount,
                        ExpenseTypeId = dto.ExpenseTypeId,
                        PaymentAccountId = dto.PaymentAccountId,
                        Status = "Posted", // Auto-post for now
                        CreatedBy = userId
                    };

                    _context.Expenses.Add(expense);
                    await _context.SaveChangesAsync();

                    // Post to Accounting
                    await _accountingService.PostExpenseTransactionAsync(expense.Id, userId);

                    await transaction.CommitAsync();

                    return MapToDto(expense, expenseType.Name, paymentAccount.AccountName);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<IEnumerable<ExpenseDto>> GetExpensesAsync(Guid companyId, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Expenses
                .Include(e => e.ExpenseType)
                .Include(e => e.PaymentAccount)
                .Where(e => e.CompanyId == companyId);

            if (startDate.HasValue) query = query.Where(e => e.ExpenseDate >= startDate.Value);
            if (endDate.HasValue) query = query.Where(e => e.ExpenseDate <= endDate.Value);

            var expenses = await query.OrderByDescending(e => e.ExpenseDate).ToListAsync();

            return expenses.Select(e => MapToDto(e, e.ExpenseType?.Name ?? "Unknown", e.PaymentAccount?.AccountName ?? "Unknown")).ToList();
        }

        private static ExpenseDto MapToDto(Expense e, string expTypeName, string payAccName)
        {
            return new ExpenseDto
            {
                Id = e.Id,
                ExpenseDate = e.ExpenseDate,
                Description = e.Description,
                ReferenceNumber = e.ReferenceNumber,
                Amount = e.Amount,
                ExpenseTypeId = e.ExpenseTypeId,
                ExpenseTypeName = expTypeName,
                PaymentAccountId = e.PaymentAccountId,
                PaymentAccountName = payAccName,
                Status = e.Status
            };
        }
    }
}
