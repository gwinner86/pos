using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Common.Models;
using POS.Application.DTOs.Pricing;
using POS.Application.Interfaces;

namespace POS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PricingController : ControllerBase
    {
        private readonly IPricingService _pricingService;
        private readonly IValidator<CreatePricingDto> _createValidator;
        private readonly IValidator<UpdatePricingDto> _updateValidator;

        public PricingController(
            IPricingService pricingService,
            IValidator<CreatePricingDto> createValidator,
            IValidator<UpdatePricingDto> updateValidator)
        {
            _pricingService = pricingService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<PricingDto>>>> GetPricings()
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
            {
                 return Unauthorized(ApiResponse<object>.FailureResponse("User does not belong to a company.", 401));
            }

            try
            {
                var pricings = await _pricingService.GetPricingsAsync(tenantId, companyId);
                return Ok(ApiResponse<IEnumerable<PricingDto>>.SuccessResponse(pricings, "Pricing records retrieved."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.FailureResponse(ex.Message, 500));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<PricingDto>>> GetPricing(Guid id)
        {
             var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
             try
             {
                 var pricing = await _pricingService.GetPricingByIdAsync(id, tenantId);
                 return Ok(ApiResponse<PricingDto>.SuccessResponse(pricing, "Pricing record retrieved."));
             }
             catch (KeyNotFoundException ex)
             {
                 return NotFound(ApiResponse<object>.FailureResponse(ex.Message, 404));
             }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<PricingDto>>> CreatePricing([FromBody] CreatePricingDto request)
        {
            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return BadRequest(ApiResponse<object>.FailureResponse(errors, 400));
            }

            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
            {
                 return Unauthorized(ApiResponse<object>.FailureResponse("User does not belong to a company.", 401));
            }

            try
            {
                var pricing = await _pricingService.CreatePricingAsync(request, tenantId, userId, companyId);
                return StatusCode(201, ApiResponse<PricingDto>.SuccessResponse(pricing, "Pricing record created successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                 return BadRequest(ApiResponse<object>.FailureResponse(ex.Message, 400));
            }
            catch (Exception ex)
            {
                 return StatusCode(500, ApiResponse<object>.FailureResponse(ex.Message, 500));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<PricingDto>>> UpdatePricing(Guid id, [FromBody] UpdatePricingDto request)
        {
             var validationResult = await _updateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return BadRequest(ApiResponse<object>.FailureResponse(errors, 400));
            }

            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);

            try
            {
                var pricing = await _pricingService.UpdatePricingAsync(id, request, tenantId, userId);
                return Ok(ApiResponse<PricingDto>.SuccessResponse(pricing, "Pricing record updated successfully."));
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
        public async Task<ActionResult<ApiResponse<object>>> DeletePricing(Guid id, [FromQuery] string reason = "Soft Delete")
        {
             var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
             var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);

             var deleted = await _pricingService.DeletePricingAsync(id, tenantId, userId, reason);
             
             if (!deleted)
             {
                 return NotFound(ApiResponse<object>.FailureResponse("Pricing record not found.", 404));
             }

             return Ok(ApiResponse<object>.SuccessResponse(null, "Pricing record deactivated (soft deleted)."));
        }
    }
}
