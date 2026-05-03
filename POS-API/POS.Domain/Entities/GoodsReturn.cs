using POS.Domain.Common;

namespace POS.Domain.Entities
{
    public class GoodsReturn : BaseEntity
    {
        public Guid CompanyId { get; set; }
        public Guid LocationId { get; set; }
        public Guid UserId { get; set; }
        public Guid? SalesOrPurchaseId { get; set; }
        public string ReturnType { get; set; } = "SALE_RETURN";
        public DateTime ReturnDate { get; set; } = DateTime.UtcNow;
        public decimal TotalRefundAmount { get; set; }
        public string? ReturnReason { get; set; }
        public Guid CreatedBy { get; set; }

        public Company? Company { get; set; }
        public Location? Location { get; set; }
        public User? User { get; set; }
        public ICollection<GoodsReturnedDetail> Details { get; set; } = new List<GoodsReturnedDetail>();
    }
}
