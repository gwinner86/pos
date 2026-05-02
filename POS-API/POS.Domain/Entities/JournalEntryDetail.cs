using POS.Domain.Common;

namespace POS.Domain.Entities
{
    public class JournalEntryDetail : BaseEntity
    {
        public Guid JournalHeaderId { get; set; }
        public Guid GLAccountId { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }

        public JournalEntry? JournalEntry { get; set; }
        public GLAccount? GLAccount { get; set; }
    }
}
