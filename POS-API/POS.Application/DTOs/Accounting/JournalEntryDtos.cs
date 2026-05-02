namespace POS.Application.DTOs.Accounting
{
    public class JournalEntryDto
    {
        public Guid Id { get; set; }
        public DateTime EntryDate { get; set; }
        public string SourceTable { get; set; } = string.Empty;
        public Guid SourceId { get; set; }
        public string? Description { get; set; }
        public bool IsPosted { get; set; }
        public DateTime? PostedDate { get; set; }
        public List<JournalEntryDetailDto> Details { get; set; } = new();
    }

    public class JournalEntryDetailDto
    {
        public Guid GLAccountId { get; set; }
        public string AccountName { get; set; } = string.Empty; // For reading
        public string AccountNumber { get; set; } = string.Empty; // For reading
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
    }

    public class CreateJournalEntryDto
    {
        public DateTime EntryDate { get; set; } = DateTime.UtcNow;
        public string SourceTable { get; set; } = string.Empty;
        public Guid SourceId { get; set; }
        public string? Description { get; set; }
        public List<CreateJournalEntryDetailDto> Details { get; set; } = new();
    }

    public class CreateJournalEntryDetailDto
    {
        public string AccountNumber { get; set; } = string.Empty; // We identify by Code for ease
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
    }
}
