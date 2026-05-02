using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using POS.Application.DTOs.Auth;
using POS.Application.Interfaces;
using POS.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;

namespace POS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IValidator<RegisterTenantRequest> _validator;

        public AuthController(IAuthService authService, IValidator<RegisterTenantRequest> validator)
        {
            _authService = authService;
            _validator = validator;
        }

        [HttpPost("register-tenant")]
        public async Task<ActionResult<ApiResponse<object>>> RegisterTenant([FromBody] RegisterTenantRequest request)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return BadRequest(ApiResponse<object>.FailureResponse(errors, 400));
            }

            try
            {
                var response = await _authService.RegisterTenantAsync(request);
                return Ok(ApiResponse<RegisterTenantResponse>.SuccessResponse(response, "Tenant and Admin registered successfully.", 201));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? "";
                return StatusCode(500, ApiResponse<object>.FailureResponse($"An error occurred: {ex.Message} {innerMessage}", 500));
            }
        }
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password)) // Basic validation
            {
                 return BadRequest(ApiResponse<object>.FailureResponse("Email and Password are required.", 400));
            }

            try
            {
                var response = await _authService.LoginAsync(request);
                return Ok(ApiResponse<LoginResponse>.SuccessResponse(response, "Login successful.", 200));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<object>.FailureResponse(ex.Message, 401));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.FailureResponse($"An error occurred: {ex.Message}", 500));
            }
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<ActionResult<ApiResponse<object>>> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var currentUserId))
            {
                return Unauthorized(ApiResponse<object>.FailureResponse("Invalid User ID in token.", 401));
            }

            try
            {
                await _authService.ChangePasswordAsync(currentUserId, request);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Password successfully changed.", 200));
            }
            catch (UnauthorizedAccessException ex)
            {
                return BadRequest(ApiResponse<object>.FailureResponse(ex.Message, 400));
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


        [Authorize]
        [HttpPost("register-user")]
        public async Task<ActionResult<ApiResponse<object>>> RegisterUser([FromBody] RegisterUserRequest request)
        {
            // Extract User ID from Claims for Auditing
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var currentUserId))
            {
                return Unauthorized(ApiResponse<object>.FailureResponse("Invalid User ID in token.", 401));
            }

            // Optional: Verify TenantId match
            var tenantIdClaim = User.FindFirst("TenantId");
            if (tenantIdClaim != null && Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            {
               if(request.TenantId != tenantId)
               {
                   return Forbid("Cannot create user for another tenant.");
               }
            }

            try
            {
                var userId = await _authService.RegisterUserAsync(request, currentUserId);
                return Ok(ApiResponse<object>.SuccessResponse(new { UserId = userId }, "User registered successfully.", 201));
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
        [Authorize]
        [HttpPut("users/{userId}")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateUserProfile(Guid userId, [FromBody] UpdateUserDto request)
        {
             // Verify user is Updating themselves OR has Admin rights (Simplified to Self-Updater for now or Admin check)
             // For now, let's assume if they have a valid token they can try, but we should check IDs.
             var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
             if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var currentUserId))
             {
                 return Unauthorized(ApiResponse<object>.FailureResponse("Invalid User ID in token.", 401));
             }

             // In a real app, authorize if currentUserId == userId OR currentUserId is Admin
             // Implementing flexible check:
             
             try
             {
                 await _authService.UpdateUserAsync(userId, request, currentUserId);
                 return Ok(ApiResponse<object>.SuccessResponse(null, "User profile updated successfully.", 200));
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

        [Authorize]
        [HttpPatch("users/{userId}/status")]
        public async Task<ActionResult<ApiResponse<object>>> ToggleUserStatus(Guid userId, [FromQuery] bool isActive)
        {
             var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
             if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var currentUserId))
             {
                 return Unauthorized(ApiResponse<object>.FailureResponse("Invalid User ID in token.", 401));
             }

             try
             {
                 await _authService.ToggleUserStatusAsync(userId, isActive, currentUserId);
                 var status = isActive ? "activated" : "deactivated";
                 return Ok(ApiResponse<object>.SuccessResponse(null, $"User has been {status}.", 200));
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
        [Authorize]
        [HttpGet("users")]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserResponse>>>> GetUsers([FromQuery] bool includeDeleted = false)
        {
            var tenantIdClaim = User.FindFirst("TenantId");
            if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            {
                return Unauthorized(ApiResponse<object>.FailureResponse("Invalid Tenant ID in token.", 401));
            }

            try
            {
                var users = await _authService.GetUsersAsync(tenantId, includeDeleted);
                return Ok(ApiResponse<IEnumerable<UserResponse>>.SuccessResponse(users, "Users retrieved successfully.", 200));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.FailureResponse($"An error occurred: {ex.Message}", 500));
            }
        }

        [Authorize]
        [HttpGet("users/deleted")]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserResponse>>>> GetDeletedUsers()
        {
            var tenantIdClaim = User.FindFirst("TenantId");
            if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            {
                return Unauthorized(ApiResponse<object>.FailureResponse("Invalid Tenant ID in token.", 401));
            }

            try
            {
                var users = await _authService.GetDeletedUsersAsync(tenantId);
                return Ok(ApiResponse<IEnumerable<UserResponse>>.SuccessResponse(users, "Deleted users retrieved successfully.", 200));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.FailureResponse($"An error occurred: {ex.Message}", 500));
            }
        }

        [Authorize]
        [HttpDelete("users/{userId}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteUser(Guid userId)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var currentUserId))
            {
                return Unauthorized(ApiResponse<object>.FailureResponse("Invalid User ID in token.", 401));
            }

            try
            {
                await _authService.SoftDeleteUserAsync(userId, currentUserId);
                return Ok(ApiResponse<object>.SuccessResponse(null, "User deleted successfully.", 200));
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

        [Authorize]
        [HttpPost("users/{userId}/reset-password")]
        public async Task<ActionResult<ApiResponse<object>>> ResetUserPassword(Guid userId)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var currentUserId))
            {
                return Unauthorized(ApiResponse<object>.FailureResponse("Invalid User ID in token.", 401));
            }

            try
            {
                var newPassword = await _authService.ResetUserPasswordAsync(userId, currentUserId);
                // Ideally, we don't return the password in response, expecting email delivery. 
                // But for development/testing visibility:
                return Ok(ApiResponse<object>.SuccessResponse(new { TemporaryPassword = newPassword }, "Password reset successfully. Check email.", 200));
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
        [Authorize]
        [HttpPost("switch-company/{companyId}")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> SwitchCompany(Guid companyId)
        {
             var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
             if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var currentUserId))
             {
                 return Unauthorized(ApiResponse<object>.FailureResponse("Invalid User ID.", 401));
             }

             try
             {
                 var response = await _authService.SwitchCompanyAsync(currentUserId, companyId);
                 return Ok(ApiResponse<LoginResponse>.SuccessResponse(response, "Switched company successfully."));
             }
             catch (UnauthorizedAccessException ex)
             {
                 return Unauthorized(ApiResponse<object>.FailureResponse(ex.Message, 403));
             }
             catch (Exception ex)
             {
                 return StatusCode(500, ApiResponse<object>.FailureResponse($"An error occurred: {ex.Message}", 500));
             }
        }
    }
}
