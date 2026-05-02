using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.DTOs.Accounting;
using POS.Application.Interfaces;
using POS.Infrastructure.Persistence; // For CompanyId resolution helper access if needed, or stick to pattern.
using Microsoft.EntityFrameworkCore; // needed for helper
using System.Security.Claims;

namespace POS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountingController : ControllerBase
    {
        private readonly IAccountingService _accountingService;
        private readonly ApplicationDbContext _context; // Injecting context only for quick helper CompanyId resolution

        public AccountingController(IAccountingService accountingService, ApplicationDbContext context)
        {
            _accountingService = accountingService;
            _context = context;
        }

        [HttpGet("accounts")]
        public async Task<ActionResult<IEnumerable<GLAccountDto>>> GetGLAccounts()
        {
            try
            {
                var userId = GetUserId();
                var companyId = await GetCompanyIdForUser(userId);
                
                // Ensure defaults exist before returning (Lazy Seeding)
                await _accountingService.EnsureDefaultAccountsAsync(companyId, userId);

                var accounts = await _accountingService.GetGLAccountsAsync(companyId);
                return Ok(accounts);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("journals")]
        public async Task<ActionResult<IEnumerable<JournalEntryDto>>> GetJournalEntries()
        {
            try
            {
                var userId = GetUserId();
                var companyId = await GetCompanyIdForUser(userId);
                
                var journals = await _accountingService.GetJournalEntriesAsync(companyId);
                return Ok(journals);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpPost("journal-entry")]
        public async Task<ActionResult<JournalEntryDto>> CreateJournalEntry([FromBody] CreateJournalEntryDto dto)
        {
            try 
            {
                var userId = GetUserId();
                var companyId = await GetCompanyIdForUser(userId);
                
                var result = await _accountingService.PostJournalEntryAsync(dto, userId, companyId);
                return Ok(result);
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

        // Helper to resolve company from logged in user
        private async Task<Guid> GetCompanyIdForUser(Guid userId)
        {
             var assignment = await _context.UserCompanyAssignments
                .FirstOrDefaultAsync(uca => uca.UserId == userId);
            
            if (assignment == null)
                throw new UnauthorizedAccessException("User is not assigned to any company.");
                
            return assignment.CompanyId;
        }
    }
}
