using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.DTOs.Settings;
using POS.Application.Interfaces;
using System.Security.Claims;

namespace POS.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SetupCountryController : ControllerBase
    {
        private readonly ISetupCountryService _setupCountryService;

        public SetupCountryController(ISetupCountryService setupCountryService)
        {
            _setupCountryService = setupCountryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var companyIdString = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdString) || !Guid.TryParse(companyIdString, out var companyId))
            {
                return BadRequest(new { Message = "CompanyId is required" });
            }

            var countries = await _setupCountryService.GetAllSetupCountriesAsync(companyId);
            return Ok(countries);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var country = await _setupCountryService.GetSetupCountryByIdAsync(id);
            if (country == null) return NotFound();
            return Ok(country);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSetupCountryDto request)
        {
            var companyIdString = User.FindFirst("CompanyId")?.Value;
            var tenantIdString = User.FindFirst("TenantId")?.Value;

            if (string.IsNullOrEmpty(companyIdString) || !Guid.TryParse(companyIdString, out var companyId))
            {
                return BadRequest(new { Message = "CompanyId is required" });
            }
            if (string.IsNullOrEmpty(tenantIdString) || !Guid.TryParse(tenantIdString, out var tenantId))
            {
                 return BadRequest(new { Message = "TenantId is required" });
            }

            request.CompanyId = companyId;
            request.TenantId = tenantId;

            var result = await _setupCountryService.CreateSetupCountryAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateSetupCountryDto request)
        {
            try
            {
                var result = await _setupCountryService.UpdateSetupCountryAsync(id, request);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _setupCountryService.DeleteSetupCountryAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
