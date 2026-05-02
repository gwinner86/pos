using POS.Domain.Common;

namespace POS.Domain.Entities
{
    public class SetupExpenseType : BaseEntity
    {
        public Guid TenantId { get; set; }
        public Guid CompanyId { get; set; }
        
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        
        public Guid ExpenseAccountId { get; set; } // GL Account mapped to this type
        public GLAccount? ExpenseAccount { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
