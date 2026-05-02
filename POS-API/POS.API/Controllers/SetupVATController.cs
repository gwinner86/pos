using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Common.Models;
using POS.Application.DTOs.Settings;
using POS.Application.Interfaces;

namespace POS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SetupVATController : ControllerBase
    {
        private readonly ISetupVATService _setupVATService;

        public SetupVATController(ISetupVATService setupVATService)
        {
            _setupVATService = setupVATService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<SetupVATDto>>>> GetAll()
        {
            var tenantIdClaim = User.FindFirst("TenantId")?.Value;
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;

            if (string.IsNullOrEmpty(tenantIdClaim) || string.IsNullOrEmpty(companyIdClaim))
            {
                return Unauthorized(ApiResponse<object>.FailureResponse("Tenant or Company identifier is missing.", 401));
            }

            var tenantId = Guid.Parse(tenantIdClaim);
            var companyId = Guid.Parse(companyIdClaim);

            try
            {
                var vats = await _setupVATService.GetAllAsync(tenantId, companyId);
                return Ok(ApiResponse<IEnumerable<SetupVATDto>>.SuccessResponse(vats, "VAT configurations retrieved."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.FailureResponse(ex.Message, 500));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<SetupVATDto>>> GetById(Guid id)
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);

            try
            {
                var vat = await _setupVATService.GetByIdAsync(id, tenantId);
                return Ok(ApiResponse<SetupVATDto>.SuccessResponse(vat, "VAT configuration retrieved."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.FailureResponse(ex.Message, 404));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.FailureResponse(ex.Message, 500));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<SetupVATDto>>> Create([FromBody] CreateSetupVATDto request)
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);
            
            // Allow overriding companyId from claim if not passed
            if (request.CompanyId == Guid.Empty)
            {
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (!string.IsNullOrEmpty(companyIdClaim) && Guid.TryParse(companyIdClaim, out var companyId))
                {
                    request.CompanyId = companyId;
                }
            }

            try
            {
                var vat = await _setupVATService.CreateAsync(request, tenantId, userId);
                return CreatedAtAction(nameof(GetById), new { id = vat.Id }, ApiResponse<SetupVATDto>.SuccessResponse(vat, "VAT configuration created."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.FailureResponse(ex.Message, 500));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<SetupVATDto>>> Update(Guid id, [FromBody] UpdateSetupVATDto request)
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);

            // Allow overriding companyId from claim if not passed
            if (request.CompanyId == Guid.Empty)
            {
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (!string.IsNullOrEmpty(companyIdClaim) && Guid.TryParse(companyIdClaim, out var companyId))
                {
                    request.CompanyId = companyId;
                }
            }

            try
            {
                var vat = await _setupVATService.UpdateAsync(id, request, tenantId, userId);
                return Ok(ApiResponse<SetupVATDto>.SuccessResponse(vat, "VAT configuration updated."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.FailureResponse(ex.Message, 404));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.FailureResponse(ex.Message, 500));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);

            try
            {
                await _setupVATService.DeleteAsync(id, tenantId);
                return Ok(ApiResponse<object>.SuccessResponse(null, "VAT configuration deleted."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.FailureResponse(ex.Message, 404));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.FailureResponse(ex.Message, 500));
            }
        }
    }
}
