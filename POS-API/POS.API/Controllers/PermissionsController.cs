using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Common.Models;
using POS.Application.DTOs.Permission;
using POS.Application.Interfaces;

namespace POS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Require auth for all permission actions
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionsController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<PermissionDto>>>> GetAllPermissions()
        {
            var permissions = await _permissionService.GetAllPermissionsAsync();
            return Ok(ApiResponse<IEnumerable<PermissionDto>>.SuccessResponse(permissions, "Permissions retrieved successfully."));
        }

        [HttpGet("roles/{roleId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PermissionDto>>>> GetRolePermissions(int roleId)
        {
            var permissions = await _permissionService.GetPermissionsByRoleIdAsync(roleId);
            return Ok(ApiResponse<IEnumerable<PermissionDto>>.SuccessResponse(permissions, "Role permissions retrieved successfully."));
        }

        [HttpPost("roles/assign")]
        public async Task<ActionResult<ApiResponse<object>>> AssignPermissions([FromBody] AssignPermissionsDto request)
        {
            // Optional: Check if user is Admin / Has 'ManagePermissions' permission
            // For now, we rely on broad [Authorize]
            
            try
            {
                await _permissionService.AssignPermissionsToRoleAsync(request.RoleId, request.PermissionIds);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Permissions assigned successfully."));
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
    }
}
