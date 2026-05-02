namespace POS.Application.DTOs.Accounting
{
    public class GLAccountDto
    {
        public Guid Id { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public bool DebitIncreases { get; set; }
        public bool IsActive { get; set; }
        public decimal Balance { get; set; }
    }
}
