using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Interfaces;
using System.Security.Claims;

namespace POS.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IAccountingService _accountingService;

        public ReportsController(IAccountingService accountingService)
        {
            _accountingService = accountingService;
        }

        [HttpGet("income-statement")]
        public async Task<IActionResult> GetIncomeStatement([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var companyIdClaim = User.FindFirst("CompanyId");
            if (companyIdClaim == null) return BadRequest("Company ID not found in token.");
            var companyId = Guid.Parse(companyIdClaim.Value);

            var start = startDate ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var end = endDate ?? DateTime.UtcNow;

            var report = await _accountingService.GetIncomeStatementAsync(companyId, start, end);
            return Ok(report);
        }

        [HttpGet("balance-sheet")]
        public async Task<IActionResult> GetBalanceSheet([FromQuery] DateTime? asOfDate)
        {
            var companyIdClaim = User.FindFirst("CompanyId");
            if (companyIdClaim == null) return BadRequest("Company ID not found in token.");
            var companyId = Guid.Parse(companyIdClaim.Value);

            var date = asOfDate ?? DateTime.UtcNow;

            var report = await _accountingService.GetBalanceSheetAsync(companyId, date);
            return Ok(report);
        }

        [HttpGet("trial-balance")]
        public async Task<IActionResult> GetTrialBalance([FromQuery] DateTime? asOfDate)
        {
            var companyIdClaim = User.FindFirst("CompanyId");
            if (companyIdClaim == null) return BadRequest("Company ID not found in token.");
            var companyId = Guid.Parse(companyIdClaim.Value);

            var date = asOfDate ?? DateTime.UtcNow;

            var report = await _accountingService.GetTrialBalanceAsync(companyId, date);
            return Ok(report);
        }
    }
}
