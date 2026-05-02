using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.DTOs.PaymentMethod;
using POS.Application.Interfaces;
using System.Security.Claims;

namespace POS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentMethodsController : ControllerBase
    {
        private readonly IPaymentMethodService _service;

        public PaymentMethodsController(IPaymentMethodService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<PaymentMethodDto>>> GetAll()
        {
            var userId = GetUserId();
            var result = await _service.GetAllPaymentMethodsForUserAsync(userId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentMethodDto>> GetById(int id)
        {
            try
            {
                var result = await _service.GetPaymentMethodByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<ActionResult<PaymentMethodDto>> Create([FromBody] CreatePaymentMethodDto dto)
        {
            var userId = GetUserId();
            var result = await _service.CreatePaymentMethodAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = result.PaymentMethodId }, result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PaymentMethodDto>> Update(int id, [FromBody] UpdatePaymentMethodDto dto)
        {
            if (id != dto.PaymentMethodId) return BadRequest("ID Mismatch");
            var userId = GetUserId();
            
            try 
            {
                var result = await _service.UpdatePaymentMethodAsync(dto, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                await _service.DeletePaymentMethodAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        private Guid GetUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim == null) throw new UnauthorizedAccessException();
            return Guid.Parse(idClaim.Value);
        }
    }
}
