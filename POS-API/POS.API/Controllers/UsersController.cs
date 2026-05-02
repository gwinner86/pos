using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Common.Models;
using POS.Application.DTOs.Company;
using POS.Application.DTOs.Location;
using POS.Application.Interfaces;
using System.Security.Claims;

namespace POS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IAuthService _authService; // Using IAuthService as it holds the logic

        public UsersController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet("me/companies")]
        public async Task<ActionResult<ApiResponse<IEnumerable<CompanyDto>>>> GetMyCompanies()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                 return Unauthorized(ApiResponse<object>.FailureResponse("Invalid User ID.", 401));
            }

            try
            {
                var companies = await _authService.GetUserCompaniesAsync(userId);
                return Ok(ApiResponse<IEnumerable<CompanyDto>>.SuccessResponse(companies, "Companies retrieved successfully.", 200));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.FailureResponse($"An error occurred: {ex.Message}", 500));
            }
        }

        [HttpGet("me/companies/{companyId}/locations")]
        public async Task<ActionResult<ApiResponse<IEnumerable<LocationResponse>>>> GetMyCompanyLocations(Guid companyId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                 return Unauthorized(ApiResponse<object>.FailureResponse("Invalid User ID.", 401));
            }

            try
            {
                var locations = await _authService.GetUserLocationsAsync(userId, companyId);
                return Ok(ApiResponse<IEnumerable<LocationResponse>>.SuccessResponse(locations, "Locations retrieved successfully.", 200));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.FailureResponse($"An error occurred: {ex.Message}", 500));
            }
        }
    }
}
