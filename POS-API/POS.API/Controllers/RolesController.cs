using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Common.Models;
using POS.Application.DTOs.Role;
using POS.Application.Interfaces;

namespace POS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly IPermissionService _permissionService;
        private readonly IValidator<CreateRoleDto> _validator;

        public RolesController(IRoleService roleService, IPermissionService permissionService, IValidator<CreateRoleDto> validator)
        {
            _roleService = roleService;
            _permissionService = permissionService;
            _validator = validator;
        }

        private Guid GetTenantId()
        {
            var claim = User.FindFirst("TenantId");
            return claim != null && Guid.TryParse(claim.Value, out var id) ? id : Guid.Empty;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<RoleResponse>>>> GetRoles()
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty) return Unauthorized(ApiResponse<object>.FailureResponse("Tenant ID missing.", 401));

            var roles = await _roleService.GetRolesAsync(tenantId);
            return Ok(ApiResponse<IEnumerable<RoleResponse>>.SuccessResponse(roles, "Roles retrieved successfully.", 200));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<RoleResponse>>> GetRole(int id)
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty) return Unauthorized(ApiResponse<object>.FailureResponse("Tenant ID missing.", 401));

            var role = await _roleService.GetRoleByIdAsync(id, tenantId);
            if (role == null) return NotFound(ApiResponse<object>.FailureResponse("Role not found.", 404));

            return Ok(ApiResponse<RoleResponse>.SuccessResponse(role, "Role retrieved.", 200));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<object>>> CreateRole([FromBody] CreateRoleDto request)
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty) return Unauthorized(ApiResponse<object>.FailureResponse("Tenant ID missing.", 401));

            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                 var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                 return BadRequest(ApiResponse<object>.FailureResponse(errors, 400));
            }

            try
            {
                var roleId = await _roleService.CreateRoleAsync(request, tenantId);
                return StatusCode(201, ApiResponse<object>.SuccessResponse(new { RoleId = roleId }, "Role created.", 201));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.FailureResponse(ex.Message, 400));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateRole(int id, [FromBody] UpdateRoleDto request)
        {
             var tenantId = GetTenantId();
             if (tenantId == Guid.Empty) return Unauthorized(ApiResponse<object>.FailureResponse("Tenant ID missing.", 401));

             try
             {
                 var success = await _roleService.UpdateRoleAsync(id, request, tenantId);
                 if (!success) return NotFound(ApiResponse<object>.FailureResponse("Role not found.", 404));

                 return Ok(ApiResponse<object>.SuccessResponse(null, "Role updated.", 200));
             }
             catch (InvalidOperationException ex)
             {
                 return BadRequest(ApiResponse<object>.FailureResponse(ex.Message, 400));
             }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteRole(int id)
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty) return Unauthorized(ApiResponse<object>.FailureResponse("Tenant ID missing.", 401));

             try
             {
                 var success = await _roleService.DeleteRoleAsync(id, tenantId);
                 if (!success) return NotFound(ApiResponse<object>.FailureResponse("Role not found.", 404));

                 return Ok(ApiResponse<object>.SuccessResponse(null, "Role deleted.", 200));
             }
             catch (InvalidOperationException ex)
             {
                 return BadRequest(ApiResponse<object>.FailureResponse(ex.Message, 400));
             }
        }

        // ============================
        // Permissions
        // ============================

        [HttpGet("permissions")]
        public async Task<ActionResult<ApiResponse<object>>> GetAllPermissions()
        {
            var permissions = await _permissionService.GetAllPermissionsAsync();
            return Ok(ApiResponse<object>.SuccessResponse(permissions, "Permissions retrieved.", 200));
        }

        [HttpGet("{id}/permissions")]
        public async Task<ActionResult<ApiResponse<object>>> GetRolePermissions(int id)
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty) return Unauthorized(ApiResponse<object>.FailureResponse("Tenant ID missing.", 401));

            // Optional: check if role belongs to tenant
            var role = await _roleService.GetRoleByIdAsync(id, tenantId);
            if (role == null) return NotFound(ApiResponse<object>.FailureResponse("Role not found.", 404));

            var permissions = await _permissionService.GetPermissionsByRoleIdAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(permissions, "Role permissions retrieved.", 200));
        }

        [HttpPut("{id}/permissions")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateRolePermissions(int id, [FromBody] List<int> permissionIds)
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty) return Unauthorized(ApiResponse<object>.FailureResponse("Tenant ID missing.", 401));

            // Check if role belongs to tenant
            var role = await _roleService.GetRoleByIdAsync(id, tenantId);
            if (role == null) return NotFound(ApiResponse<object>.FailureResponse("Role not found.", 404));

            try
            {
                await _permissionService.AssignPermissionsToRoleAsync(id, permissionIds);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Role permissions updated successfully.", 200));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.FailureResponse(ex.Message, 404));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.FailureResponse(ex.Message, 400));
            }
        }
    }
}
