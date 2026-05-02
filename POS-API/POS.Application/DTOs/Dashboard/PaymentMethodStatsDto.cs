namespace POS.Application.DTOs.Dashboard
{
    public class PaymentMethodStatsDto
    {
        public string PaymentMethod { get; set; } = string.Empty;
        public int TransactionCount { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
