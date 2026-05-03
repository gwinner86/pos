using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Common.Models;
using POS.Application.DTOs.Location;
using POS.Application.Interfaces;

namespace POS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LocationsController : ControllerBase
    {
        private readonly ILocationService _locationService;
        private readonly IValidator<CreateLocationDto> _validator;

        public LocationsController(ILocationService locationService, IValidator<CreateLocationDto> validator)
        {
            _locationService = locationService;
            _validator = validator;
        }

        private Guid GetTenantId()
        {
            var claim = User.FindFirst("TenantId");
            return claim != null && Guid.TryParse(claim.Value, out var id) ? id : Guid.Empty;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<LocationResponse>>>> GetLocations()
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty) return Unauthorized(ApiResponse<object>.FailureResponse("Tenant ID missing.", 401));

            var locations = await _locationService.GetLocationsAsync(tenantId);
            return Ok(ApiResponse<IEnumerable<LocationResponse>>.SuccessResponse(locations, "Locations retrieved successfully.", 200));
        }

        [HttpGet("company/{companyId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<LocationResponse>>>> GetLocationsByCompany(Guid companyId)
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty) return Unauthorized(ApiResponse<object>.FailureResponse("Tenant ID missing.", 401));

            var locations = await _locationService.GetLocationsByCompanyIdAsync(companyId, tenantId);
            return Ok(ApiResponse<IEnumerable<LocationResponse>>.SuccessResponse(locations, "Locations retrieved successfully.", 200));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<LocationResponse>>> GetLocation(Guid id)
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty) return Unauthorized(ApiResponse<object>.FailureResponse("Tenant ID missing.", 401));

            var location = await _locationService.GetLocationByIdAsync(id, tenantId);
            if (location == null) return NotFound(ApiResponse<object>.FailureResponse("Location not found.", 404));

            return Ok(ApiResponse<LocationResponse>.SuccessResponse(location, "Location retrieved.", 200));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<object>>> CreateLocation([FromBody] CreateLocationDto request)
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty) return Unauthorized(ApiResponse<object>.FailureResponse("Tenant ID missing.", 401));

            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return BadRequest(ApiResponse<object>.FailureResponse(errors, 400));
            }

            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
            {
                return Unauthorized(ApiResponse<object>.FailureResponse("User does not belong to a company.", 401));
            }

            try
            {
                var locationId = await _locationService.CreateLocationAsync(request, tenantId, companyId);
                return StatusCode(201, ApiResponse<object>.SuccessResponse(new { Id = locationId }, "Location created.", 201));
            }
            catch (KeyNotFoundException ex)
            {
                 return NotFound(ApiResponse<object>.FailureResponse(ex.Message, 404));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.FailureResponse($"An error occurred: {ex.Message}", 500));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateLocation(Guid id, [FromBody] UpdateLocationDto request)
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty) return Unauthorized(ApiResponse<object>.FailureResponse("Tenant ID missing.", 401));

            var success = await _locationService.UpdateLocationAsync(id, request, tenantId);
            if (!success) return NotFound(ApiResponse<object>.FailureResponse("Location not found.", 404));

            return Ok(ApiResponse<object>.SuccessResponse(null, "Location updated.", 200));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteLocation(Guid id)
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty) return Unauthorized(ApiResponse<object>.FailureResponse("Tenant ID missing.", 401));

            var success = await _locationService.DeleteLocationAsync(id, tenantId);
            if (!success) return NotFound(ApiResponse<object>.FailureResponse("Location not found.", 404));

            return Ok(ApiResponse<object>.SuccessResponse(null, "Location deleted.", 200));
        }
    }
}
