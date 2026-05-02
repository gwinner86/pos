using POS.Domain.Common;

namespace POS.Domain.Entities
{
    public class JournalEntry : BaseEntity
    {
        public Guid TenantId { get; set; }
        public Guid CompanyId { get; set; }
        public Guid LocationId { get; set; }
        public DateTime EntryDate { get; set; } = DateTime.UtcNow;
        public string SourceTable { get; set; } = string.Empty;
        public Guid SourceId { get; set; }
        public string? Description { get; set; }
        public bool IsPosted { get; set; } = false;
        public DateTime? PostedDate { get; set; }
        public Guid CreatedBy { get; set; }

        public Company? Company { get; set; }
        public Location? Location { get; set; }
        public ICollection<JournalEntryDetail> Details { get; set; } = new List<JournalEntryDetail>();
    }
}
