using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs.Dashboard;
using POS.Application.DTOs.Sale;
using POS.Application.Interfaces;
using POS.Infrastructure.Persistence;

namespace POS.Infrastructure.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync(Guid tenantId, Guid companyId, string? filter = null)
        {
            var today = DateTime.UtcNow.Date;
            DateTime? startDate = null;
            DateTime? endDate = null;

            switch (filter?.ToLower())
            {
                case "today":
                    startDate = today;
                    endDate = today.AddDays(1).AddTicks(-1);
                    break;
                case "yesterday":
                    startDate = today.AddDays(-1);
                    endDate = today.AddTicks(-1);
                    break;
                case "last_week":
                    startDate = today.AddDays(-7);
                    endDate = today.AddDays(1).AddTicks(-1);
                    break;
                case "last_month":
                    startDate = today.AddDays(-30);
                    endDate = today.AddDays(1).AddTicks(-1);
                    break;
                default:
                    // Default to lifetime/all-time if no filter
                    break;
            }

            // 1. Total Revenue (Completed sales only)
            var totalRevenueQuery = _context.Sales
                .Where(s => s.TenantId == tenantId && s.CompanyId == companyId && s.Status != "Cancelled" && s.Status == "Completed");
            
            if (startDate.HasValue) totalRevenueQuery = totalRevenueQuery.Where(s => s.SaleDate >= startDate.Value);
            if (endDate.HasValue) totalRevenueQuery = totalRevenueQuery.Where(s => s.SaleDate <= endDate.Value);
            
            var totalRevenue = await totalRevenueQuery.SumAsync(s => s.TotalAmount);

            // 2. Sales Count (All non-cancelled?)
            var salesCountQuery = _context.Sales
                .Where(s => s.TenantId == tenantId && s.CompanyId == companyId && s.Status == "Completed");
                
            if (startDate.HasValue) salesCountQuery = salesCountQuery.Where(s => s.SaleDate >= startDate.Value);
            if (endDate.HasValue) salesCountQuery = salesCountQuery.Where(s => s.SaleDate <= endDate.Value);
            
            var salesCount = await salesCountQuery.CountAsync();

            // 3. Active Products (Lifetime, non-filtered)
            var activeProducts = await _context.Products
                .Where(p => p.TenantId == tenantId && p.CompanyId == companyId && p.IsActive)
                .CountAsync();

            // 4. Active Customers (Lifetime, non-filtered)
            var activeCustomers = await _context.Customers
                .Where(c => c.TenantId == tenantId && c.CompanyId == companyId && c.IsActive)
                .CountAsync();

            // 5. Recent Sales
            var recentSalesQuery = _context.Sales
                .Include(s => s.Customer)
                .Where(s => s.TenantId == tenantId && s.CompanyId == companyId);
                
            if (startDate.HasValue) recentSalesQuery = recentSalesQuery.Where(s => s.SaleDate >= startDate.Value);
            if (endDate.HasValue) recentSalesQuery = recentSalesQuery.Where(s => s.SaleDate <= endDate.Value);

            var recentSales = await recentSalesQuery
                .OrderByDescending(s => s.SaleDate)
                .Take(5)
                .Select(s => new SaleDto
                {
                    Id = s.Id,
                    SaleDate = s.SaleDate,
                    SaleNumber = s.SaleNumber,
                    TotalAmount = s.TotalAmount,
                    PaymentMethod = s.PaymentMethod,
                    Status = s.Status,
                    CustomerName = s.Customer != null ? (s.Customer.FirstName + " " + (s.Customer.LastName ?? "")).Trim() : "Walk-in Customer",
                    CustomerId = s.CustomerId
                })
                .ToListAsync();

            // 6. Sales Trend (Last 30 days)
            // Note: GroupBy in EF Core sometimes has limitations with Dates. Fetching relevant data first then grouping in memory if necessary, or using simple GroupBy.
            // For simplicity and compatibility, we'll fetch last 30 days data and group in memory (unless volume is huge).
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            
            var rawSalesData = await _context.Sales
                .Where(s => s.TenantId == tenantId && s.CompanyId == companyId && s.SaleDate >= thirtyDaysAgo && s.Status != "Cancelled")
                 .Select(s => new { s.SaleDate, s.TotalAmount })
                .ToListAsync();

            var salesTrend = rawSalesData
                .GroupBy(s => s.SaleDate.Date)
                .Select(g => new SalesTrendDto
                {
                    Date = g.Key.ToString("MMM dd"), // e.g. "Jan 01"
                    TotalSales = g.Sum(s => s.TotalAmount)
                })
                .OrderBy(x => DateTime.ParseExact(x.Date, "MMM dd", System.Globalization.CultureInfo.InvariantCulture)) // This sort might be tricky with just Month Day. Better to keep Key and sort, then project.
                // Let's retry sort strategy:
                // Actually, the Result list should be sorted by date.
                .ToList();
            
            // Re-sort properly
            salesTrend = rawSalesData
                .GroupBy(s => s.SaleDate.Date)
                .OrderBy(g => g.Key)
                .Select(g => new SalesTrendDto
                {
                    Date = g.Key.ToString("dd MMM", System.Globalization.CultureInfo.InvariantCulture), // "08 Jan"
                    TotalSales = g.Sum(s => s.TotalAmount)
                })
                .ToList();

            // 7. Fast & Slow Moving Products
            var productSalesQuery = _context.SaleDetails
                .Include(sd => sd.Sale)
                .Include(sd => sd.ProductVariant)
                .ThenInclude(v => v.Product)
                .Where(sd => sd.Sale!.TenantId == tenantId && sd.Sale.CompanyId == companyId && sd.Sale.Status == "Completed");

            if (startDate.HasValue) productSalesQuery = productSalesQuery.Where(sd => sd.Sale!.SaleDate >= startDate.Value);
            if (endDate.HasValue) productSalesQuery = productSalesQuery.Where(sd => sd.Sale!.SaleDate <= endDate.Value);

            var productPerformance = await productSalesQuery
                .GroupBy(sd => new { sd.ProductVariant!.Product!.Id, sd.ProductVariant.Product.ProductName, sd.ProductVariant.VariantSku })
                .Select(g => new ProductPerformanceDto
                {
                    ProductId = g.Key.Id,
                    ProductName = g.Key.ProductName,
                    Sku = g.Key.VariantSku ?? "N/A",
                    QuantitySold = (int)g.Sum(sd => sd.Quantity),
                    TotalRevenue = g.Sum(sd => sd.LineTotal)
                })
                .ToListAsync();

            var fastMoving = productPerformance
                .OrderByDescending(p => p.QuantitySold)
                .Take(5)
                .ToList();

            var slowMoving = productPerformance
                .OrderBy(p => p.QuantitySold)
                .Take(5)
                .ToList();

            // 8. Payment Method Stats
            var paymentStatsQuery = _context.Sales
                .Where(s => s.TenantId == tenantId && s.CompanyId == companyId && s.Status == "Completed");

            if (startDate.HasValue) paymentStatsQuery = paymentStatsQuery.Where(s => s.SaleDate >= startDate.Value);
            if (endDate.HasValue) paymentStatsQuery = paymentStatsQuery.Where(s => s.SaleDate <= endDate.Value);

            var paymentStats = await paymentStatsQuery
                .GroupBy(s => s.PaymentMethod)
                .Select(g => new PaymentMethodStatsDto
                {
                    PaymentMethod = g.Key ?? "Unknown",
                    TransactionCount = g.Count(),
                    TotalAmount = g.Sum(s => s.TotalAmount)
                })
                .ToListAsync();

            return new DashboardStatsDto
            {
                TotalRevenue = totalRevenue,
                SalesCount = salesCount,
                ActiveProductsCount = activeProducts,
                ActiveCustomersCount = activeCustomers,
                RecentSales = recentSales,
                SalesTrend = salesTrend,
                FastMovingProducts = fastMoving,
                SlowMovingProducts = slowMoving,
                PaymentMethodStats = paymentStats
            };
        }
    }
}
