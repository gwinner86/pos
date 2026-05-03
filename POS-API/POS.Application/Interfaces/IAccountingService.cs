using POS.Application.DTOs.Accounting;
using POS.Application.DTOs.Expense;
using POS.Application.DTOs.GoodsReturn;
using POS.Application.DTOs.Inventory;
using POS.Application.DTOs.InvoicePayment;
using POS.Application.DTOs.Sale;
using POS.Application.DTOs.SalePayment;
using POS.Application.DTOs.SupplierInvoice;

namespace POS.Application.Interfaces
{
    public interface IAccountingService
    {
        Task EnsureDefaultAccountsAsync(Guid companyId, Guid userId); // Seeder
        Task<IEnumerable<GLAccountDto>> GetGLAccountsAsync(Guid companyId);
        
        Task<JournalEntryDto> PostJournalEntryAsync(CreateJournalEntryDto dto, Guid userId, Guid companyId);
        Task<IEnumerable<JournalEntryDto>> GetJournalEntriesAsync(Guid companyId);
        
        // High Level Integrations
        Task PostSaleTransactionAsync(SaleDto saleDto, Guid userId);
        Task PostDirectSaleTransactionAsync(SaleDto saleDto, Guid userId); // Immediate Cash Sale
        Task PostPaymentTransactionAsync(SalePaymentDto paymentDto, Guid userId);
        Task PostReturnTransactionAsync(GoodsReturnDto returnDto, Guid userId);
        Task PostInventoryAdjustmentAsync(Guid inventoryId, decimal quantityChanged, string reason, Guid userId);
        Task PostExpenseTransactionAsync(Guid expenseId, Guid userId);
        Task PostSupplierInvoiceTransactionAsync(SupplierInvoiceDto invoice, Guid userId);
        Task PostInvoicePaymentTransactionAsync(InvoicePaymentDto payment, Guid userId);

        // Reports
        Task<IncomeStatementDto> GetIncomeStatementAsync(Guid companyId, DateTime startDate, DateTime endDate);
        Task<BalanceSheetDto> GetBalanceSheetAsync(Guid companyId, DateTime asOfDate);
        Task<TrialBalanceDto> GetTrialBalanceAsync(Guid companyId, DateTime asOfDate);
    }
}
