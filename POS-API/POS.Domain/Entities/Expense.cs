using POS.Domain.Common;
using POS.Domain.Entities;

namespace POS.Domain.Entities
{
    public class Expense : BaseEntity
    {
        public Guid CompanyId { get; set; }
        public Guid TenantId { get; set; }
        public Guid LocationId { get; set; }
        
        public DateTime ExpenseDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? ReferenceNumber { get; set; }
        public decimal Amount { get; set; }

        // Accounting Links
        public Guid ExpenseTypeId { get; set; } // The Type driving the GL Account
        public Guid PaymentAccountId { get; set; } // The GL Account paid from (Credit, e.g. Cash/Bank)
        
        public string Status { get; set; } = "Draft"; // Draft, Posted

        public Guid CreatedBy { get; set; }

        // Navigation
        public SetupExpenseType? ExpenseType { get; set; }
        public GLAccount? PaymentAccount { get; set; }
    }
}
