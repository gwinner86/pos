namespace POS.Application.DTOs.Dashboard
{
    public class SalesTrendDto
    {
        public string Date { get; set; } = string.Empty; // "Jan 01", "Mon", etc.
        public decimal TotalSales { get; set; }
    }
}
