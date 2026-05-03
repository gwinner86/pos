using Microsoft.AspNetCore.Mvc;
using POS.Application.DTOs.Tenant;
using POS.Application.Interfaces;
using POS.Application.Common.Models;

namespace POS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantsController : ControllerBase
    {
        private readonly ITenantService _tenantService;
        private readonly FluentValidation.IValidator<CreateTenantDto> _validator;

        public TenantsController(ITenantService tenantService, FluentValidation.IValidator<CreateTenantDto> validator)
        {
            _tenantService = tenantService;
            _validator = validator;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<TenantDto>>>> GetAll()
        {
            var tenants = await _tenantService.GetAllTenantsAsync();
            return Ok(ApiResponse<IEnumerable<TenantDto>>.SuccessResponse(tenants, "Tenants retrieved successfully."));
        }

        [HttpGet("by-name/{name}")]
        public async Task<ActionResult<ApiResponse<TenantDto>>> GetByName(string name)
        {
            var tenant = await _tenantService.GetTenantByNameAsync(name);
            if (tenant == null) 
                return NotFound(ApiResponse<TenantDto>.FailureResponse("Tenant not found.", 404));
            
            return Ok(ApiResponse<TenantDto>.SuccessResponse(tenant, "Tenant retrieved successfully."));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<TenantDto>>> GetById(Guid id)
        {
            var tenant = await _tenantService.GetTenantByIdAsync(id);
            if (tenant == null) 
                return NotFound(ApiResponse<TenantDto>.FailureResponse("Tenant not found.", 404));
            
            return Ok(ApiResponse<TenantDto>.SuccessResponse(tenant, "Tenant retrieved successfully."));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<string>>> Create([FromBody] CreateTenantDto createTenantDto)
        {
            var validationResult = await _validator.ValidateAsync(createTenantDto);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return BadRequest(ApiResponse<string>.FailureResponse(errors, 400));
            }

            await _tenantService.CreateTenantAsync(createTenantDto);
            // Returning null data as requested
            return Ok(ApiResponse<string>.SuccessResponse(null, "Tenant created successfully.", 201));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> Update(Guid id, [FromBody] UpdateTenantDto updateTenantDto)
        {
            try
            {
                await _tenantService.UpdateTenantAsync(id, updateTenantDto);
                return Ok(ApiResponse<object>.SuccessResponse(new { }, "Tenant updated successfully."));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponse<object>.FailureResponse("Tenant not found.", 404));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
        {
            try
            {
                await _tenantService.DeleteTenantAsync(id);
                return Ok(ApiResponse<object>.SuccessResponse(new { }, "Tenant deleted successfully."));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponse<object>.FailureResponse("Tenant not found.", 404));
            }
        }
    }
}
