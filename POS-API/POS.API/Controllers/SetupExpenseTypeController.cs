using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.DTOs.SetupExpenseType;
using POS.Application.Interfaces;
using System.Security.Claims;

namespace POS.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SetupExpenseTypeController : ControllerBase
    {
        private readonly ISetupExpenseTypeService _service;

        public SetupExpenseTypeController(ISetupExpenseTypeService service)
        {
            _service = service;
        }

        private Guid GetTenantId() => Guid.Parse(User.FindFirstValue("TenantId") ?? Guid.Empty.ToString());
        private Guid GetCompanyId() => Guid.Parse(User.FindFirstValue("CompanyId") ?? Guid.Empty.ToString());

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSetupExpenseTypeRequest request)
        {
            try
            {
                var result = await _service.CreateAsync(request, GetTenantId(), GetCompanyId());
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSetupExpenseTypeRequest request)
        {
            try
            {
                var result = await _service.UpdateAsync(id, request, GetTenantId(), GetCompanyId());
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _service.GetByIdAsync(id, GetTenantId(), GetCompanyId());
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
        {
            var result = await _service.GetAllAsync(GetTenantId(), GetCompanyId(), includeInactive);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _service.DeleteAsync(id, GetTenantId(), GetCompanyId());
                return NoContent();
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }
    }
}
