using POS.Domain.Common;

namespace POS.Domain.Entities
{
    public class GLAccount : BaseEntity
    {
        public Guid CompanyId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public bool DebitIncreases { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid CreatedBy { get; set; }

        public Company? Company { get; set; }
    }
}
