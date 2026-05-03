using POS.Application.DTOs.Auth;
using POS.Application.DTOs.Company;
using POS.Application.DTOs.Location;

namespace POS.Application.Interfaces
{
    public interface IAuthService
    {
        Task<RegisterTenantResponse> RegisterTenantAsync(RegisterTenantRequest request);
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<Guid> RegisterUserAsync(RegisterUserRequest request, Guid createdByUserId);
        Task<bool> UpdateUserAsync(Guid userId, UpdateUserDto request, Guid updatedBy);
        Task<bool> ToggleUserStatusAsync(Guid userId, bool isActive, Guid updatedBy);
        Task<bool> SoftDeleteUserAsync(Guid userId, Guid deletedBy);
        Task<IEnumerable<UserResponse>> GetUsersAsync(Guid tenantId, bool includeDeleted = false);
        Task<IEnumerable<UserResponse>> GetDeletedUsersAsync(Guid tenantId);
        Task<string> ResetUserPasswordAsync(Guid userId, Guid adminId);
        Task<IEnumerable<CompanyDto>> GetUserCompaniesAsync(Guid userId);
        Task<IEnumerable<LocationResponse>> GetUserLocationsAsync(Guid userId, Guid companyId);
        Task<LoginResponse> SwitchCompanyAsync(Guid userId, Guid companyId);
        Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    }
}
