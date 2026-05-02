using POS.Application.DTOs.Auth;
using POS.Application.DTOs.Company;
using POS.Application.DTOs.Location;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Infrastructure.Persistence;
using BCrypt.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace POS.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<RegisterTenantResponse> RegisterTenantAsync(RegisterTenantRequest request)
        {
            // Validate Feature Identity before transaction to fail fast
            var featureExists = await _context.Features.AnyAsync(f => f.FeatureId == request.FeatureId);
            if (!featureExists) 
            {
                throw new KeyNotFoundException($"Feature with ID {request.FeatureId} not found.");
            }

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 1. Create Tenant
                    var tenant = new Tenant
                    {
                        TenantName = request.TenantName,
                        FeatureId = request.FeatureId,
                        IsActive = true
                    };
                    _context.Tenants.Add(tenant);
                    await _context.SaveChangesAsync();

                    // 2. Create Company
                    var company = new Company
                    {
                        TenantId = tenant.Id,
                        CompanyName = request.CompanyName,
                        CompanyPrimaryPhoneNumber = request.CompanyPrimaryPhoneNumber,
                        CompanyEmail = request.CompanyEmailAddress,
                        FeatureId = request.FeatureId, // assigning same feature to company initially?
                        IsActive = true
                    };
                    _context.Companies.Add(company);
                    await _context.SaveChangesAsync();

                    // 3. Create Default Location/Branch
                    var location = new Location
                    {
                        TenantId = tenant.Id,
                        CompanyId = company.Id,
                        LocationName = request.LocationName,
                        LocationType = "Branch",
                        FeatureId = request.FeatureId
                    };
                    _context.Locations.Add(location);
                    await _context.SaveChangesAsync();

                    // 4. Create or Get Admin Role (Scoped to this Tenant/Company?) 
                    // Creating a standard "Admin" role for this Tenant
                    var adminRole = await _context.Roles
                        .Include(r => r.RolePermissions)
                        .FirstOrDefaultAsync(r => r.TenantId == tenant.Id && r.RoleName == "Admin");

                    if (adminRole == null)
                    {
                        adminRole = new Role
                        {
                            RoleName = "Admin",
                            TenantId = tenant.Id,
                            CompanyId = company.Id,
                            Description = "Default Admin Role with full access"
                        };
                        _context.Roles.Add(adminRole);
                        await _context.SaveChangesAsync();
                    }

                    // Assign ALL permissions to this Admin role so menus are fully visible
                    var allPermissions = await _context.Permissions.ToListAsync();
                    var existingPermissionIds = adminRole.RolePermissions?.Select(rp => rp.PermissionId).ToHashSet() ?? new HashSet<int>();
                    var permissionsToAssign = allPermissions
                        .Where(p => !existingPermissionIds.Contains(p.PermissionId))
                        .Select(p => new RolePermission
                        {
                            RoleId = adminRole.RoleId,
                            PermissionId = p.PermissionId
                        })
                        .ToList();

                    if (permissionsToAssign.Any())
                    {
                        _context.RolePermissions.AddRange(permissionsToAssign);
                        await _context.SaveChangesAsync();
                    }

                    // 5. Create User
                    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
                    var user = new User
                    {
                        TenantId = tenant.Id,
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        Email = request.Email,
                        PasswordHash = hashedPassword,
                        IsActive = true,
                        // CreatedBy could be the user itself (self-reference) or null for system
                    };
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                    
                    // Update User CreatedBy to self?
                    user.CreatedBy = user.Id;
                    _context.Users.Update(user);

                    // 6. Assign User to Company with Admin Role
                    var assignment = new UserCompanyAssignment
                    {
                        UserId = user.Id,
                        CompanyId = company.Id,
                        RoleId = adminRole.RoleId,
                        LocationId = location.Id
                    };
                    _context.UserCompanyAssignments.Add(assignment);
                    await _context.SaveChangesAsync(); // Final save

                    await transaction.CommitAsync();

                    return new RegisterTenantResponse
                    {
                        UserId = user.Id,
                        TenantId = tenant.Id,
                        CompanyId = company.Id
                    };
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            Console.WriteLine($"[LOGIN ATTEMPT] Email: {request.Email}, TenantId: {request.TenantId}");

            var userQuery = _context.Users
                .Include(u => u.Assignments)
                .AsQueryable();

            if (request.TenantId.HasValue && request.TenantId.Value != Guid.Empty)
            {
                userQuery = userQuery.Where(u => u.Email == request.Email && u.TenantId == request.TenantId.Value);
            }
            else if (!string.IsNullOrEmpty(request.TenantName))
            {
                // Resolve Tenant by Name
                var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.TenantName == request.TenantName);
                if (tenant == null)
                {
                     Console.WriteLine($"[LOGIN FAILED] Tenant not found: {request.TenantName}");
                     throw new UnauthorizedAccessException("Invalid tenant name.");
                }
                userQuery = userQuery.Where(u => u.Email == request.Email && u.TenantId == tenant.Id);
            }
            else
            {
                userQuery = userQuery.Where(u => u.Email == request.Email);
            }

            var user = await userQuery.FirstOrDefaultAsync();

            if (user == null)
            {
                Console.WriteLine($"[LOGIN FAILED] User not found for email: {request.Email}");
                throw new UnauthorizedAccessException("Invalid email or tenant.");
            }

            Console.WriteLine($"[LOGIN SUCCESS] User found: {user.Id}");

            if (!user.IsActive)
            {
                Console.WriteLine($"[LOGIN FAILED] User inactive.");
                throw new UnauthorizedAccessException("User account is inactive.");
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                Console.WriteLine($"[LOGIN FAILED] Invalid password.");
                throw new UnauthorizedAccessException("Invalid password.");
            }

            var token = GenerateJwtToken(user);
            
            Console.WriteLine($"[LOGIN SUCCESS] Token generated.");

            var assignment = user.Assignments.FirstOrDefault();
            var permissionsList = new List<string>();
            var roleName = "";

            if (assignment?.RoleId != null)
            {
                var role = await _context.Roles
                    .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(r => r.RoleId == assignment.RoleId);
                
                if (role != null)
                {
                    roleName = role.RoleName;
                    permissionsList = role.RolePermissions
                        .Where(rp => rp.Permission != null)
                        .Select(rp => rp.Permission.Name)
                        .ToList();
                }
            }

            return new LoginResponse
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(24), 
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                TenantId = user.TenantId,
                Role = roleName,
                CompanyId = assignment?.CompanyId,
                RequiresPasswordChange = user.RequiresPasswordChange,
                Permissions = permissionsList
            };
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JWT SecretKey is not configured.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("TenantId", user.TenantId.ToString()),
                new Claim("CompanyId", user.Assignments.FirstOrDefault()?.CompanyId.ToString() ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public async Task<Guid> RegisterUserAsync(RegisterUserRequest request, Guid createdByUserId)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Verify Tenant Exists
                    var tenantExists = await _context.Tenants.AnyAsync(t => t.Id == request.TenantId);
                    if (!tenantExists) throw new KeyNotFoundException("Tenant not found.");

                    // Create User
                    // 1. Generate Random Password
                    var generatedPassword = Guid.NewGuid().ToString().Substring(0, 8); // Simple 8 char password for now
                    // TODO: Replace with better password generator

                    // 2. Mock Email Sending (Log to Console)
                    Console.WriteLine($"[MOCK EMAIL] Sending welcome email to {request.Email}. Temporary Password: {generatedPassword}");

                    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(generatedPassword);
                    var user = new User
                    {
                        TenantId = request.TenantId,
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        Email = request.Email,
                        PasswordHash = hashedPassword,
                        IsActive = true,
                        RequiresPasswordChange = true, // Force reset
                        CreatedBy = createdByUserId
                    };
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    // Assign User to Company
                    var roleExists = await _context.Roles.AnyAsync(r => r.RoleId == request.RoleId && r.TenantId == request.TenantId);
                    if (!roleExists) throw new KeyNotFoundException($"Role with ID {request.RoleId} not found for this tenant.");

                    var assignment = new UserCompanyAssignment
                    {
                        UserId = user.Id,
                        CompanyId = request.CompanyId,
                        RoleId = request.RoleId,
                    };
                    _context.UserCompanyAssignments.Add(assignment);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return user.Id;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
        public async Task<bool> UpdateUserAsync(Guid userId, UpdateUserDto request, Guid updatedBy)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) throw new KeyNotFoundException("User not found.");

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.UpdatedBy = updatedBy;
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleUserStatusAsync(Guid userId, bool isActive, Guid updatedBy)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) throw new KeyNotFoundException("User not found.");

            user.IsActive = isActive;
            user.UpdatedBy = updatedBy;
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> SoftDeleteUserAsync(Guid userId, Guid deletedBy)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) throw new KeyNotFoundException("User not found.");

            user.IsActive = false;
            user.DeletedBy = deletedBy;
            user.DeletedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<UserResponse>> GetUsersAsync(Guid tenantId, bool includeDeleted = false)
        {
            var query = _context.Users
                .Include(u => u.Assignments)
                .ThenInclude(a => a.Role)
                .Where(u => u.TenantId == tenantId);
                
            if (!includeDeleted)
            {
                 query = query.Where(u => u.IsActive);
            }

            var users = await query.ToListAsync();

            return users.Select(u => new UserResponse
            {
                UserId = u.Id,
                FirstName = u.FirstName ?? "",
                LastName = u.LastName ?? "",
                Email = u.Email,
                RoleName = u.Assignments.FirstOrDefault()?.Role?.RoleName ?? "No Role",
                IsActive = u.IsActive,
                RequiresPasswordChange = u.RequiresPasswordChange,
                TenantId = u.TenantId,
                CompanyId = u.Assignments.FirstOrDefault()?.CompanyId
            });
        }

        public async Task<IEnumerable<UserResponse>> GetDeletedUsersAsync(Guid tenantId)
        {
            var users = await _context.Users
                .Where(u => u.TenantId == tenantId && !u.IsActive && u.DeletedAt != null)
                .ToListAsync();
            
            // Get names of deleters
            var deleterIds = users.Where(u => u.DeletedBy.HasValue).Select(u => u.DeletedBy.Value).Distinct().ToList();
            var deleters = await _context.Users.Where(u => deleterIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => $"{u.FirstName} {u.LastName}");

            return users.Select(u => new UserResponse
            {
                UserId = u.Id,
                FirstName = u.FirstName ?? "",
                LastName = u.LastName ?? "",
                Email = u.Email,
                IsActive = u.IsActive,
                RequiresPasswordChange = u.RequiresPasswordChange,
                TenantId = u.TenantId,
                DeletedAt = u.DeletedAt,
                DeletedByName = u.DeletedBy.HasValue && deleters.ContainsKey(u.DeletedBy.Value) ? deleters[u.DeletedBy.Value] : "Unknown"
            });
        }

        public async Task<string> ResetUserPasswordAsync(Guid userId, Guid adminId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) throw new KeyNotFoundException("User not found.");

            // 1. Generate Random Password
            var generatedPassword = Guid.NewGuid().ToString().Substring(0, 8);
            // TODO: Use better generator

            // 2. Mock Email
            Console.WriteLine($"[MOCK EMAIL] Password Reset for {user.Email}. New Temporary Password: {generatedPassword}");

            // 3. Update User
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(generatedPassword);
            user.RequiresPasswordChange = true;
            user.UpdatedBy = adminId;
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return generatedPassword;
        }

        public async Task<IEnumerable<CompanyDto>> GetUserCompaniesAsync(Guid userId)
        {
            var user = await _context.Users
                .Include(u => u.Assignments)
                .ThenInclude(a => a.Company)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) throw new KeyNotFoundException("User not found.");

            var companies = user.Assignments
                .Select(a => a.Company)
                .Where(c => c != null && c.IsActive)
                .GroupBy(c => c.Id) // DistinctBy might not be available in EF Core query translation depending on version, GroupBy is safer
                .Select(g => g.First())
                .Select(c => new CompanyDto
                {
                    CompanyId = c.Id,
                    CompanyName = c.CompanyName,
                    TenantId = c.TenantId,
                    FeatureId = c.FeatureId,
                    CompanyAddress = c.CompanyAddress,
                    CompanyEmail = c.CompanyEmail,
                    CompanyOtherPhoneNumbers = c.CompanyOtherPhoneNumbers,
                    CompanyPrimaryPhoneNumber = c.CompanyPrimaryPhoneNumber,
                    TaxId = c.TaxId,
                    IsActive = c.IsActive
                });

            return companies;
        }

        public async Task<IEnumerable<LocationResponse>> GetUserLocationsAsync(Guid userId, Guid companyId)
        {
            var assignments = await _context.UserCompanyAssignments
               .Include(a => a.Role)
               .Where(a => a.UserId == userId && a.CompanyId == companyId)
               .ToListAsync();

            if (!assignments.Any()) return new List<LocationResponse>();

            // Admins or users with null LocationId have full access to ALL locations
            bool isAdmin = assignments.Any(a => a.Role != null && a.Role.RoleName == "Admin");
            bool hasFullAccess = isAdmin || assignments.Any(a => a.LocationId == null);

            var query = _context.Locations.Where(l => l.CompanyId == companyId);

            if (!hasFullAccess)
            {
                var allowedLocationIds = assignments
                    .Where(a => a.LocationId != null)
                    .Select(a => a.LocationId.Value)
                    .ToList();
                
                query = query.Where(l => allowedLocationIds.Contains(l.Id));
            }

            var locations = await query
                .Include(l => l.Currency)
                .Select(l => new LocationResponse
            {
                Id = l.Id,
                LocationName = l.LocationName,
                TenantId = l.TenantId,
                CompanyId = l.CompanyId,
                FeatureId = l.FeatureId,
                LocationType = l.LocationType,
                AddressLine1 = l.AddressLine1,
                VatCalculationType = l.VatCalculationType,
                CurrencyId = l.CurrencyId,
                CurrencySymbol = l.Currency != null ? l.Currency.CurrencySymbol : null,
                CurrencyCode = l.Currency != null ? l.Currency.CurrencyCode : null
            }).ToListAsync();

            return locations;
        }
        public async Task<LoginResponse> SwitchCompanyAsync(Guid userId, Guid companyId)
        {
            var user = await _context.Users
                .Include(u => u.Assignments)
                .ThenInclude(a => a.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);
            
            if (user == null) throw new KeyNotFoundException("User not found.");

            // Check if user is Admin in this tenant
            var isAdmin = user.Assignments.Any(a => a.Role != null && a.Role.RoleName == "Admin" && a.Role.TenantId == user.TenantId);

            // Verify assignment or Admin privileges
            var assignment = user.Assignments.FirstOrDefault(a => a.CompanyId == companyId);
            
            if (assignment == null)
            {
                if (!isAdmin)
                {
                    throw new UnauthorizedAccessException("User does not have access to this company.");
                }

                // Verify the target company actually belongs to the tenant
                var companyExists = await _context.Companies.AnyAsync(c => c.Id == companyId && c.TenantId == user.TenantId);
                if (!companyExists)
                {
                     throw new UnauthorizedAccessException("Company does not belong to this tenant.");
                }
            }

            // Generate new token with this companyId
            // We need to modify GenerateJwtToken to accept companyId or rely on user object having it?
            // GenerateJwtToken uses: user.Assignments.FirstOrDefault()?.CompanyId.
            // We should overload GenerateJwtToken or pass the specific companyId.
            
            var token = GeneratJwtTokenForCompany(user, companyId);

            var roleName = "";
            var permissionsList = new List<string>();

            if (assignment != null)
            {
                var role = await _context.Roles
                    .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(r => r.RoleId == assignment.RoleId);

                if (role != null)
                {
                    roleName = role.RoleName;
                    permissionsList = role.RolePermissions
                        .Where(rp => rp.Permission != null)
                        .Select(rp => rp.Permission.Name)
                        .ToList();
                }
            }
            else if (isAdmin)
            {
                roleName = "Admin"; // Fallback for implicit admin access
                var adminRole = user.Assignments.FirstOrDefault(a => a.Role != null && a.Role.RoleName == "Admin" && a.Role.TenantId == user.TenantId)?.Role;
                if (adminRole != null)
                {
                    var fullAdminRole = await _context.Roles
                        .Include(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                        .FirstOrDefaultAsync(r => r.RoleId == adminRole.RoleId);
                    
                    if (fullAdminRole != null)
                    {
                        permissionsList = fullAdminRole.RolePermissions
                            .Where(rp => rp.Permission != null)
                            .Select(rp => rp.Permission.Name)
                            .ToList();
                    }
                }
            }

            return new LoginResponse
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(24),
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName ?? "",
                LastName = user.LastName ?? "",
                TenantId = user.TenantId,
                Role = roleName,
                CompanyId = companyId,
                RequiresPasswordChange = user.RequiresPasswordChange,
                Permissions = permissionsList
            };
        }

        private string GeneratJwtTokenForCompany(User user, Guid companyId)
        {
             var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JWT SecretKey is not configured.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("TenantId", user.TenantId.ToString()),
                new Claim("CompanyId", companyId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Incorrect old password.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.RequiresPasswordChange = false;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
