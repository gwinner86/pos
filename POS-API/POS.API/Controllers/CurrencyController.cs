using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.DTOs.Currency;
using POS.Application.Interfaces;
using System.Security.Claims;

namespace POS.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CurrencyController : ControllerBase
    {
        private readonly ICurrencyService _currencyService;

        public CurrencyController(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCurrencies()
        {
            var companyId = Guid.Parse(User.FindFirst("CompanyId")?.Value ?? Guid.Empty.ToString());
            if (companyId == Guid.Empty) return BadRequest("Company ID not found in token.");

            var currencies = await _currencyService.GetCurrenciesAsync(companyId);
            return Ok(currencies);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCurrency([FromBody] SetCurrencyDto request)
        {
            var companyId = Guid.Parse(User.FindFirst("CompanyId")?.Value ?? Guid.Empty.ToString());
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

            if (companyId == Guid.Empty) return BadRequest("Company ID not found in token.");

            try
            {
                var result = await _currencyService.CreateCurrencyAsync(request, companyId, userId);
                return CreatedAtAction(nameof(GetCurrencies), result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCurrency(Guid id, [FromBody] SetCurrencyDto request)
        {
            var companyId = Guid.Parse(User.FindFirst("CompanyId")?.Value ?? Guid.Empty.ToString());
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

            if (companyId == Guid.Empty) return BadRequest("Company ID not found in token.");

            try
            {
                var result = await _currencyService.UpdateCurrencyAsync(id, request, companyId, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
