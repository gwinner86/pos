using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Common.Models;
using POS.Application.DTOs.Cost;
using POS.Application.Interfaces;

namespace POS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CostsController : ControllerBase
    {
        private readonly ICostService _costService;
        private readonly IValidator<CreateCostDto> _createValidator;
        private readonly IValidator<UpdateCostDto> _updateValidator;

        public CostsController(
            ICostService costService,
            IValidator<CreateCostDto> createValidator,
            IValidator<UpdateCostDto> updateValidator)
        {
            _costService = costService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<CostDto>>>> GetCosts()
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
            {
                 return Unauthorized(ApiResponse<object>.FailureResponse("User does not belong to a company.", 401));
            }

            try
            {
                var costs = await _costService.GetCostsAsync(tenantId, companyId);
                return Ok(ApiResponse<IEnumerable<CostDto>>.SuccessResponse(costs, "Costs retrieved."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.FailureResponse(ex.Message, 500));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CostDto>>> GetCost(Guid id)
        {
             var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
             try
             {
                 var cost = await _costService.GetCostByIdAsync(id, tenantId);
                 return Ok(ApiResponse<CostDto>.SuccessResponse(cost, "Cost record retrieved."));
             }
             catch (KeyNotFoundException ex)
             {
                 return NotFound(ApiResponse<object>.FailureResponse(ex.Message, 404));
             }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<CostDto>>> CreateCost([FromBody] CreateCostDto request)
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
                var cost = await _costService.CreateCostAsync(request, tenantId, userId, companyId);
                return StatusCode(201, ApiResponse<CostDto>.SuccessResponse(cost, "Cost record created successfully."));
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
        public async Task<ActionResult<ApiResponse<CostDto>>> UpdateCost(Guid id, [FromBody] UpdateCostDto request)
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
                var cost = await _costService.UpdateCostAsync(id, request, tenantId, userId);
                return Ok(ApiResponse<CostDto>.SuccessResponse(cost, "Cost record updated successfully."));
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
        public async Task<ActionResult<ApiResponse<object>>> DeleteCost(Guid id)
        {
             var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
             var deleted = await _costService.DeleteCostAsync(id, tenantId);
             
             if (!deleted)
             {
                 return NotFound(ApiResponse<object>.FailureResponse("Cost record not found.", 404));
             }

             return Ok(ApiResponse<object>.SuccessResponse(null, "Cost record deleted."));
        }
    }
}
