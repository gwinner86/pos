using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Common.Models;
using POS.Application.DTOs.Sale;
using POS.Application.Interfaces;

namespace POS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SalesController : ControllerBase
    {
        private readonly ISaleService _saleService;
        private readonly IValidator<CreateSaleDto> _createValidator;

        public SalesController(
            ISaleService saleService,
            IValidator<CreateSaleDto> createValidator)
        {
            _saleService = saleService;
            _createValidator = createValidator;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<SaleDto>>>> GetSales([FromQuery] Guid? customerId, [FromQuery] Guid? userId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
            {
                 return Unauthorized(ApiResponse<object>.FailureResponse("User does not belong to a company.", 401));
            }

            try
            {
                // Adjust end date to end of day if provided
                if (endDate.HasValue) 
                {
                    endDate = endDate.Value.Date.AddDays(1).AddTicks(-1);
                }

                var sales = await _saleService.GetSalesAsync(tenantId, companyId, customerId, userId, startDate, endDate);
                return Ok(ApiResponse<IEnumerable<SaleDto>>.SuccessResponse(sales, "Sales retrieved."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.FailureResponse(ex.Message, 500));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<SaleDto>>> GetSale(Guid id)
        {
             var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
             try
             {
                 var sale = await _saleService.GetSaleByIdAsync(id, tenantId);
                 return Ok(ApiResponse<SaleDto>.SuccessResponse(sale, "Sale retrieved."));
             }
             catch (KeyNotFoundException ex)
             {
                 return NotFound(ApiResponse<object>.FailureResponse(ex.Message, 404));
             }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<SaleDto>>> CreateSale([FromBody] CreateSaleDto request)
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
                var sale = await _saleService.CreateSaleAsync(request, tenantId, userId, companyId);
                return StatusCode(201, ApiResponse<SaleDto>.SuccessResponse(sale, "Sale created successfully."));
            }
            catch (InvalidOperationException ex)
            {
                 return BadRequest(ApiResponse<object>.FailureResponse(ex.Message, 400));
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
