using System.ComponentModel.DataAnnotations;

namespace POS.Application.DTOs.Expense
{
    public class ExpenseDto
    {
        public Guid Id { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? ReferenceNumber { get; set; }
        public decimal Amount { get; set; }
        
        public Guid ExpenseTypeId { get; set; }
        public string ExpenseTypeName { get; set; } = string.Empty;
        
        public Guid PaymentAccountId { get; set; }
        public string PaymentAccountName { get; set; } = string.Empty;
        
        public string Status { get; set; } = string.Empty;
    }

    public class CreateExpenseDto
    {
        [Required]
        public Guid LocationId { get; set; }
        
        [Required]
        public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        public string? ReferenceNumber { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public Guid ExpenseTypeId { get; set; } // Matches the SetupExpenseType

        [Required]
        public Guid PaymentAccountId { get; set; } // Credit
    }
}
