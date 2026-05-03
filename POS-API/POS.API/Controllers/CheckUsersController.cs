using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Infrastructure.Persistence;
using BCrypt.Net;

namespace POS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckUsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CheckUsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Join(_context.Tenants, u => u.TenantId, t => t.Id, (u, t) => new
                {
                    UserId = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    TenantName = t.TenantName,
                    IsActive = u.IsActive
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPost("reset-all-passwords")]
        public async Task<IActionResult> ResetAllPasswords()
        {
            var defaultPassword = "Password123!";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(defaultPassword);

            var users = await _context.Users.ToListAsync();
            foreach (var user in users)
            {
                user.PasswordHash = hashedPassword;
                user.RequiresPasswordChange = false; // Disable force change for easier local testing
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = $"All passwords have been reset to '{defaultPassword}'" });
        }
    }
}
