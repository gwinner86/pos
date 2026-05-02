using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.DTOs.Expense;
using POS.Application.Interfaces;
using POS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace POS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ExpensesController : ControllerBase
    {
        private readonly IExpenseService _expenseService;
        private readonly ApplicationDbContext _context;

        public ExpensesController(IExpenseService expenseService, ApplicationDbContext context)
        {
            _expenseService = expenseService;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExpenseDto>>> GetExpenses([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var userId = GetUserId();
                var companyId = await GetCompanyIdForUser(userId);
                
                var expenses = await _expenseService.GetExpensesAsync(companyId, startDate, endDate);
                return Ok(expenses);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<ExpenseDto>> CreateExpense([FromBody] CreateExpenseDto dto)
        {
            try
            {
                var userId = GetUserId();
                var companyId = await GetCompanyIdForUser(userId);
                var tenantId = await GetTenantIdForUser(userId);

                var result = await _expenseService.CreateExpenseAsync(dto, userId, tenantId, companyId);
                return CreatedAtAction(nameof(GetExpenses), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private Guid GetUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim == null) throw new UnauthorizedAccessException();
            return Guid.Parse(idClaim.Value);
        }

        private async Task<Guid> GetCompanyIdForUser(Guid userId)
        {
            var assignment = await _context.UserCompanyAssignments.FirstOrDefaultAsync(u => u.UserId == userId);
            if (assignment == null) throw new UnauthorizedAccessException("User not assigned to a company.");
            return assignment.CompanyId;
        }
        
        private async Task<Guid> GetTenantIdForUser(Guid userId)
        {
             // Simplified: Get from Company
             var companyId = await GetCompanyIdForUser(userId);
             var company = await _context.Companies.FindAsync(companyId);
             return company?.TenantId ?? Guid.Empty;
        }
    }
}
