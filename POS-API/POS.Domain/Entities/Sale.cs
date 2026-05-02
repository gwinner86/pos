using POS.Domain.Common;

namespace POS.Domain.Entities
{
    public class Sale : BaseEntity
    {
        public Guid CompanyId { get; set; }
        public Guid TenantId { get; set; }
        public Guid LocationId { get; set; }
        public Guid? CustomerId { get; set; }
        public DateTime SaleDate { get; set; } = DateTime.UtcNow;
        public string SaleNumber { get; set; } = string.Empty; // e.g. SOR-2023-0001
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Completed"; // Completed, Cancelled, Pending
        public string PaymentMethod { get; set; } = "Cash"; // Simple for now
        public Guid CreatedBy { get; set; }

        public Company? Company { get; set; }
        public Location? Location { get; set; }
        public Customer? Customer { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey("CreatedBy")]
        public User? Creator { get; set; }
        public ICollection<SaleDetail> Details { get; set; } = new List<SaleDetail>();
    }
}
