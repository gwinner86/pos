using System.ComponentModel.DataAnnotations;

namespace POS.Application.DTOs.SetupExpenseType
{
    public class SetupExpenseTypeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid ExpenseAccountId { get; set; }
        public string ExpenseAccountName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class CreateSetupExpenseTypeRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required]
        public Guid ExpenseAccountId { get; set; }
    }

    public class UpdateSetupExpenseTypeRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required]
        public Guid ExpenseAccountId { get; set; }
        public bool IsActive { get; set; }
    }
}
