using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Common.Models;
using POS.Application.DTOs.InvoicePayment;
using POS.Application.Interfaces;

namespace POS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InvoicePaymentsController : ControllerBase
    {
        private readonly IInvoicePaymentService _paymentService;
        private readonly IValidator<CreateInvoicePaymentDto> _createValidator;

        public InvoicePaymentsController(
            IInvoicePaymentService paymentService,
            IValidator<CreateInvoicePaymentDto> createValidator)
        {
            _paymentService = paymentService;
            _createValidator = createValidator;
        }

        [HttpGet("invoice/{invoiceId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<InvoicePaymentDto>>>> GetPaymentsByInvoice(Guid invoiceId)
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
            try
            {
                var payments = await _paymentService.GetPaymentsByInvoiceIdAsync(invoiceId, tenantId);
                return Ok(ApiResponse<IEnumerable<InvoicePaymentDto>>.SuccessResponse(payments, "Payments retrieved."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.FailureResponse(ex.Message, 500));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<InvoicePaymentDto>>> CreatePayment([FromBody] CreateInvoicePaymentDto request)
        {
            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return BadRequest(ApiResponse<object>.FailureResponse(errors, 400));
            }

            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);

            try
            {
                var payment = await _paymentService.CreatePaymentAsync(request, tenantId, userId);
                return StatusCode(201, ApiResponse<InvoicePaymentDto>.SuccessResponse(payment, "Payment recorded successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(ApiResponse<object>.FailureResponse(ex.Message, 400));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.FailureResponse(ex.Message, 500));
            }
        }
    }
}
