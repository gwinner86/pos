using POS.Application.DTOs.Dashboard;

namespace POS.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardStatsDto> GetDashboardStatsAsync(Guid tenantId, Guid companyId, string? filter = null);
    }
}
