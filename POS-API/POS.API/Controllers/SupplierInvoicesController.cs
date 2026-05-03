using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Common.Models;
using POS.Application.DTOs.SupplierInvoice;
using POS.Application.Interfaces;

namespace POS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SupplierInvoicesController : ControllerBase
    {
        private readonly ISupplierInvoiceService _invoiceService;
        private readonly IValidator<CreateSupplierInvoiceDto> _createValidator;
        private readonly IValidator<UpdateSupplierInvoiceDto> _updateValidator;

        public SupplierInvoicesController(
            ISupplierInvoiceService invoiceService,
            IValidator<CreateSupplierInvoiceDto> createValidator,
            IValidator<UpdateSupplierInvoiceDto> updateValidator)
        {
            _invoiceService = invoiceService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<SupplierInvoiceDto>>>> GetInvoices()
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
             var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
            {
                 return Unauthorized(ApiResponse<object>.FailureResponse("User does not belong to a company.", 401));
            }

            try
            {
                var invoices = await _invoiceService.GetInvoicesAsync(tenantId, companyId);
                return Ok(ApiResponse<IEnumerable<SupplierInvoiceDto>>.SuccessResponse(invoices, "Invoices retrieved."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.FailureResponse(ex.Message, 500));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<SupplierInvoiceDto>>> GetInvoice(Guid id)
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
            try
            {
                var invoice = await _invoiceService.GetInvoiceByIdAsync(id, tenantId);
                return Ok(ApiResponse<SupplierInvoiceDto>.SuccessResponse(invoice, "Invoice retrieved."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.FailureResponse(ex.Message, 404));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<SupplierInvoiceDto>>> CreateInvoice([FromBody] CreateSupplierInvoiceDto request)
        {
            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return BadRequest(ApiResponse<object>.FailureResponse(errors, 400));
            }

            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
            {
                 return Unauthorized(ApiResponse<object>.FailureResponse("User does not belong to a company.", 401));
            }

            try
            {
                var invoice = await _invoiceService.CreateInvoiceAsync(request, tenantId, userId, companyId);
                return StatusCode(201, ApiResponse<SupplierInvoiceDto>.SuccessResponse(invoice, "Invoice created successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                 return BadRequest(ApiResponse<object>.FailureResponse(ex.Message, 400));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<SupplierInvoiceDto>>> UpdateInvoice(Guid id, [FromBody] UpdateSupplierInvoiceDto request)
        {
            var validationResult = await _updateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                 var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return BadRequest(ApiResponse<object>.FailureResponse(errors, 400));
            }

            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);

            try
            {
                var invoice = await _invoiceService.UpdateInvoiceAsync(id, request, tenantId, userId);
                return Ok(ApiResponse<SupplierInvoiceDto>.SuccessResponse(invoice, "Invoice updated successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.FailureResponse(ex.Message, 404));
            }
        }
    }
}
