using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Common.Models;
using POS.Application.DTOs.Supplier;
using POS.Application.Interfaces;

namespace POS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SuppliersController : ControllerBase
    {
        private readonly ISupplierService _supplierService;
        private readonly IValidator<CreateSupplierDto> _createValidator;
        private readonly IValidator<UpdateSupplierDto> _updateValidator;

        public SuppliersController(
            ISupplierService supplierService, 
            IValidator<CreateSupplierDto> createValidator,
            IValidator<UpdateSupplierDto> updateValidator)
        {
            _supplierService = supplierService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<SupplierDto>>>> GetSuppliers()
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            var locationIdClaim = User.FindFirst("LocationId")?.Value;
            Guid? locationId = !string.IsNullOrEmpty(locationIdClaim) && Guid.TryParse(locationIdClaim, out var locId) ? locId : null;
            
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
            {
                 return Unauthorized(ApiResponse<object>.FailureResponse("User does not belong to a company.", 401));
            }

            try
            {
                var suppliers = await _supplierService.GetSuppliersAsync(tenantId, companyId, locationId);
                return Ok(ApiResponse<IEnumerable<SupplierDto>>.SuccessResponse(suppliers, "Suppliers retrieved."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.FailureResponse(ex.Message, 500));
            }
        }

        [HttpGet("inactive")]
        public async Task<ActionResult<ApiResponse<IEnumerable<SupplierDto>>>> GetInactiveSuppliers()
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
            {
                 return Unauthorized(ApiResponse<object>.FailureResponse("User does not belong to a company.", 401));
            }

            try
            {
                var suppliers = await _supplierService.GetInactiveSuppliersAsync(tenantId, companyId);
                return Ok(ApiResponse<IEnumerable<SupplierDto>>.SuccessResponse(suppliers, "Inactive suppliers retrieved."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.FailureResponse(ex.Message, 500));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<SupplierDto>>> GetSupplier(Guid id)
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
            try
            {
                var supplier = await _supplierService.GetSupplierByIdAsync(id, tenantId);
                return Ok(ApiResponse<SupplierDto>.SuccessResponse(supplier, "Supplier retrieved."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.FailureResponse(ex.Message, 404));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<SupplierDto>>> CreateSupplier([FromBody] CreateSupplierDto request)
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
                var supplier = await _supplierService.CreateSupplierAsync(request, tenantId, userId, companyId);
                return StatusCode(201, ApiResponse<SupplierDto>.SuccessResponse(supplier, "Supplier created successfully."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.FailureResponse(ex.Message, 400));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<SupplierDto>>> UpdateSupplier(Guid id, [FromBody] UpdateSupplierDto request)
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
                var supplier = await _supplierService.UpdateSupplierAsync(id, request, tenantId, userId);
                return Ok(ApiResponse<SupplierDto>.SuccessResponse(supplier, "Supplier updated successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.FailureResponse(ex.Message, 404));
            }
             catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.FailureResponse(ex.Message, 400));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteSupplier(Guid id)
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
            try
            {
                var isHardDeleted = await _supplierService.DeleteSupplierAsync(id, tenantId);
                var message = isHardDeleted 
                    ? "Supplier deleted successfully." 
                    : "Supplier deactivated because it has associated records.";
                
                return Ok(ApiResponse<object>.SuccessResponse(null, message));
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
