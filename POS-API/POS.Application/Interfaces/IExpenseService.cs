using POS.Application.DTOs.Expense;

namespace POS.Application.Interfaces
{
    public interface IExpenseService
    {
        Task<ExpenseDto> CreateExpenseAsync(CreateExpenseDto dto, Guid userId, Guid tenantId, Guid companyId);
        Task<IEnumerable<ExpenseDto>> GetExpensesAsync(Guid companyId, DateTime? startDate, DateTime? endDate);
    }
}
