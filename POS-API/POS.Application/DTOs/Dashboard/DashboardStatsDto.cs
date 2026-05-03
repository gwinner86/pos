using POS.Application.DTOs.Sale;

namespace POS.Application.DTOs.Dashboard
{
    public class DashboardStatsDto
    {
        public decimal TotalRevenue { get; set; }
        public int SalesCount { get; set; }
        public int ActiveProductsCount { get; set; }
        public int ActiveCustomersCount { get; set; }
        public List<SaleDto> RecentSales { get; set; } = new List<SaleDto>();
        public List<SalesTrendDto> SalesTrend { get; set; } = new List<SalesTrendDto>();
        
        // New Widgets
        public List<ProductPerformanceDto> FastMovingProducts { get; set; } = new List<ProductPerformanceDto>();
        public List<ProductPerformanceDto> SlowMovingProducts { get; set; } = new List<ProductPerformanceDto>();
        public List<PaymentMethodStatsDto> PaymentMethodStats { get; set; } = new List<PaymentMethodStatsDto>();
    }
}
