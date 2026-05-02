using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Common.Models;
using POS.Application.DTOs.PurchaseOrder;
using POS.Application.Interfaces;

namespace POS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PurchaseOrdersController : ControllerBase
    {
        private readonly IPurchaseOrderService _poService;
        private readonly IValidator<CreatePurchaseOrderDto> _createValidator;
        private readonly IValidator<UpdatePurchaseOrderDto> _updateValidator;

        public PurchaseOrdersController(
            IPurchaseOrderService poService,
            IValidator<CreatePurchaseOrderDto> createValidator,
            IValidator<UpdatePurchaseOrderDto> updateValidator)
        {
            _poService = poService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<PurchaseOrderDto>>>> GetPurchaseOrders()
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
            {
                 return Unauthorized(ApiResponse<object>.FailureResponse("User does not belong to a company.", 401));
            }

            try
            {
                var orders = await _poService.GetPurchaseOrdersAsync(tenantId, companyId);
                return Ok(ApiResponse<IEnumerable<PurchaseOrderDto>>.SuccessResponse(orders, "Purchase Orders retrieved."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.FailureResponse(ex.Message, 500));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> GetPurchaseOrder(Guid id)
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
            try
            {
                var order = await _poService.GetPurchaseOrderByIdAsync(id, tenantId);
                return Ok(ApiResponse<PurchaseOrderDto>.SuccessResponse(order, "Purchase Order retrieved."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.FailureResponse(ex.Message, 404));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> CreatePurchaseOrder([FromBody] CreatePurchaseOrderDto request)
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
                var order = await _poService.CreatePurchaseOrderAsync(request, tenantId, userId, companyId);
                return StatusCode(201, ApiResponse<PurchaseOrderDto>.SuccessResponse(order, "Purchase Order created successfully."));
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
        public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> UpdatePurchaseOrder(Guid id, [FromBody] UpdatePurchaseOrderDto request)
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
                var order = await _poService.UpdatePurchaseOrderAsync(id, request, tenantId, userId);
                return Ok(ApiResponse<PurchaseOrderDto>.SuccessResponse(order, "Purchase Order updated successfully."));
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
        [HttpPost("{id}/receive")]
        public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> ReceivePurchaseOrder(Guid id)
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);

            try
            {
                var order = await _poService.ReceivePurchaseOrderAsync(id, tenantId, userId);
                return Ok(ApiResponse<PurchaseOrderDto>.SuccessResponse(order, "Purchase Order received successfully."));
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

        [HttpPost("instant")]
        public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> InstantPurchase([FromBody] CreatePurchaseOrderDto request)
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
                var order = await _poService.InstantPurchaseAsync(request, tenantId, userId, companyId);
                return StatusCode(201, ApiResponse<PurchaseOrderDto>.SuccessResponse(order, "Instant Purchase recorded successfully."));
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
    }
}
