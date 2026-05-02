using System.ComponentModel.DataAnnotations;

namespace POS.Domain.Entities
{
    public class PaymentMethod
    {
        [Key]
        public int PaymentMethodId { get; set; }
        public Guid CompanyId { get; set; }
        public string MethodName { get; set; } = string.Empty;
        public string PaymentType { get; set; } = "CASH"; // Enum candidate
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid CreatedBy { get; set; }
        public bool IsActive { get; set; } = true;

        public Company? Company { get; set; }
    }
}
