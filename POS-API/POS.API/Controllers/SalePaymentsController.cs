using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.DTOs.SalePayment;
using POS.Application.Interfaces;
using System.Security.Claims;

namespace POS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SalePaymentsController : ControllerBase
    {
        private readonly ISalePaymentService _paymentService;

        public SalePaymentsController(ISalePaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<ActionResult<SalePaymentDto>> CreatePayment([FromBody] CreateSalePaymentDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            var userId = Guid.Parse(userIdClaim.Value);

            try
            {
                var result = await _paymentService.ProcessPaymentAsync(dto, userId);
                return CreatedAtAction(nameof(GetPaymentsBySale), new { saleId = result.SaleId }, result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("sale/{saleId}")]
        public async Task<ActionResult<List<SalePaymentDto>>> GetPaymentsBySale(Guid saleId)
        {
            var result = await _paymentService.GetPaymentsBySaleIdAsync(saleId);
            return Ok(result);
        }
    }
}
