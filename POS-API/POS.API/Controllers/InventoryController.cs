using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Common.Models;
using POS.Application.DTOs.Inventory;
using POS.Application.Interfaces;

namespace POS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly IValidator<AdjustInventoryDto> _adjustValidator;
        private readonly IValidator<CreateInventoryDto> _createValidator;

        public InventoryController(
            IInventoryService inventoryService, 
            IValidator<AdjustInventoryDto> adjustValidator,
            IValidator<CreateInventoryDto> createValidator)
        {
            _inventoryService = inventoryService;
            _adjustValidator = adjustValidator;
            _createValidator = createValidator;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<InventoryDto>>>> GetAllInventories()
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
            try
            {
                var inventories = await _inventoryService.GetAllInventoriesAsync(tenantId);
                return Ok(ApiResponse<IEnumerable<InventoryDto>>.SuccessResponse(inventories, "Inventories retrieved successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.FailureResponse(ex.Message, 500));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<InventoryDto>>> GetInventory(Guid id)
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
            try
            {
                var inventory = await _inventoryService.GetInventoryByIdAsync(id, tenantId);
                return Ok(ApiResponse<InventoryDto>.SuccessResponse(inventory, "Inventory retrieved."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.FailureResponse(ex.Message, 404));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<InventoryDto>>> CreateInventory([FromBody] CreateInventoryDto request)
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
                var inventory = await _inventoryService.CreateInventoryAsync(request, tenantId, userId, companyId);
                return StatusCode(201, ApiResponse<InventoryDto>.SuccessResponse(inventory, "Inventory created successfully."));
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

        [HttpPost("adjust")]
        public async Task<ActionResult<ApiResponse<InventoryDto>>> AdjustInventory([FromBody] AdjustInventoryDto request)
        {
            var validationResult = await _adjustValidator.ValidateAsync(request);
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
                // In some cases Inventory might not strictly need CompanyId if tenant scoped, but Products are Company scoped.
                // Safest to extract.
                 return Unauthorized(ApiResponse<object>.FailureResponse("User does not belong to a company.", 401));
            }

            try
            {
                var inventory = await _inventoryService.AdjustInventoryAsync(request, tenantId, userId, companyId);
                return Ok(ApiResponse<InventoryDto>.SuccessResponse(inventory, "Inventory adjusted successfully."));
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
