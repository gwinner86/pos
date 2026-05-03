using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Common.Models;
using POS.Application.DTOs.GoodsReceipt;
using POS.Application.Interfaces;

namespace POS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GoodsReceiptsController : ControllerBase
    {
        private readonly IGoodsReceiptService _grService;
        private readonly IValidator<CreateGoodsReceiptDto> _createValidator;
        private readonly IValidator<UpdateGoodsReceiptDto> _updateValidator;

        public GoodsReceiptsController(
            IGoodsReceiptService grService,
            IValidator<CreateGoodsReceiptDto> createValidator,
            IValidator<UpdateGoodsReceiptDto> updateValidator)
        {
            _grService = grService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<GoodsReceiptDto>>>> GetGoodsReceipts()
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
            {
                 return Unauthorized(ApiResponse<object>.FailureResponse("User does not belong to a company.", 401));
            }

            try
            {
                var receipts = await _grService.GetGoodsReceiptsAsync(tenantId, companyId);
                return Ok(ApiResponse<IEnumerable<GoodsReceiptDto>>.SuccessResponse(receipts, "Goods Receipts retrieved."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.FailureResponse(ex.Message, 500));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<GoodsReceiptDto>>> GetGoodsReceipt(Guid id)
        {
             var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
             try
             {
                 var receipt = await _grService.GetGoodsReceiptByIdAsync(id, tenantId);
                 return Ok(ApiResponse<GoodsReceiptDto>.SuccessResponse(receipt, "Goods Receipt retrieved."));
             }
             catch (KeyNotFoundException ex)
             {
                 return NotFound(ApiResponse<object>.FailureResponse(ex.Message, 404));
             }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<GoodsReceiptDto>>> CreateGoodsReceipt([FromBody] CreateGoodsReceiptDto request)
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
                var receipt = await _grService.CreateGoodsReceiptAsync(request, tenantId, userId, companyId);
                return StatusCode(201, ApiResponse<GoodsReceiptDto>.SuccessResponse(receipt, "Goods Receipt created successfully."));
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
        public async Task<ActionResult<ApiResponse<GoodsReceiptDto>>> UpdateGoodsReceipt(Guid id, [FromBody] UpdateGoodsReceiptDto request)
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
                var receipt = await _grService.UpdateGoodsReceiptAsync(id, request, tenantId, userId);
                return Ok(ApiResponse<GoodsReceiptDto>.SuccessResponse(receipt, "Goods Receipt updated successfully."));
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

        [HttpPost("{id}/approve")]
        public async Task<ActionResult<ApiResponse<GoodsReceiptDto>>> ApproveGoodsReceipt(Guid id)
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
            {
                 return Unauthorized(ApiResponse<object>.FailureResponse("User does not belong to a company.", 401));
            }

            try
            {
                var receipt = await _grService.ApproveGoodsReceiptAsync(id, tenantId, userId, companyId);
                return Ok(ApiResponse<GoodsReceiptDto>.SuccessResponse(receipt, "Goods Receipt approved successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                 return NotFound(ApiResponse<object>.FailureResponse(ex.Message, 404));
            }
            catch (InvalidOperationException ex)
            {
                 return BadRequest(ApiResponse<object>.FailureResponse(ex.Message, 400));
            }
            catch (Exception ex)
            {
                 return StatusCode(500, ApiResponse<object>.FailureResponse(ex.Message, 500));
            }
        }
    }
}
