using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.DTOs.GoodsReturn;
using POS.Application.Interfaces;
using System.Security.Claims;

namespace POS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GoodsReturnsController : ControllerBase
    {
        private readonly IGoodsReturnService _service;
        private readonly ITenantService _tenantService; // Helper to get company? Or manual helper

        public GoodsReturnsController(IGoodsReturnService service, ITenantService tenantService)
        {
            _service = service;
            _tenantService = tenantService;
        }

        [HttpPost]
        public async Task<ActionResult<GoodsReturnDto>> CreateReturn([FromBody] CreateGoodsReturnDto dto)
        {
            var userId = GetUserId();
            // Resolve CompanyId from User (similar to PaymentMethod fix)
            // Ideally, we should inject a UserContext service, but using the same pattern directly for now 
            // OR reuse the fix from PaymentMethod service if I made it public? No, I made it private.
            // I'll assume passing CompanyId via UserCompanyAssignment check here or inside service.
            // Wait, I designed the SERVICE to take 'companyId' as arg.
            // So I need to fetch it here.
            
            // To be consistent with previous fix, I should probably put this logic in a shared place or repeat it.
            // Since I can't easily refactor shared logic rapidly without breaking flow, I will fetch it via a helper method here.
            // But wait, ITenantService didn't have it.
            // I'll rely on the same pattern: I'll inject DbContext?? No, that breaks architectural layers (API shouldn't touch DbContext).
            // Better: update ISaleService / IAuthService? 
            // OR: Just let the Service handle it effectively? 
            // Actually, `GoodsReturnService` takes `companyId`.
            // I will implement a quick helper in the controller that uses `ITenantService`? No, that failed.
            // I will inject `ICompanyService`? Maybe it has it?
            // Let's look at `ICompanyService`? 
            // Time constraint: I will pass UserId to service, and let Service resolve CompanyId internally, just like I did for PaymentMethodService.
            // Checking GoodsReturnService code I just wrote...
            // "public async Task<GoodsReturnDto> CreateReturnAsync(CreateGoodsReturnDto dto, Guid userId, Guid companyId)"
            // It asks for CompanyId.
            // I will update `GoodsReturnService` to NOT require companyId and resolve it internally, matching PaymentMethodService.
            // Creating controller assuming Service is updated.
            
            try 
            {
               // Note: I will modify service in next step to resolve companyID internally to be safe.
               // For now, I'll pass Guid.Empty and fix immediately or just fix the service logic now?
               // I'll fix the service logic in the next step.
               var result = await _service.CreateReturnAsync(dto, userId); 
               return CreatedAtAction(nameof(GetReturn), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GoodsReturnDto>> GetReturn(Guid id)
        {
            try
            {
                var result = await _service.GetReturnByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("location/{locationId}")]
        public async Task<ActionResult<IEnumerable<GoodsReturnDto>>> GetByLocation(Guid locationId)
        {
            var result = await _service.GetReturnsByLocationAsync(locationId);
            return Ok(result);
        }

        private Guid GetUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim == null) throw new UnauthorizedAccessException();
            return Guid.Parse(idClaim.Value);
        }
    }
}
