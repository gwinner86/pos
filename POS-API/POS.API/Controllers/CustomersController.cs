using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Common.Models;
using POS.Application.DTOs.Customer;
using POS.Application.Interfaces;

namespace POS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IValidator<CreateCustomerDto> _createValidator;
        private readonly IValidator<UpdateCustomerDto> _updateValidator;

        public CustomersController(
            ICustomerService customerService,
            IValidator<CreateCustomerDto> createValidator,
            IValidator<UpdateCustomerDto> updateValidator)
        {
            _customerService = customerService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<CustomerDto>>>> GetCustomers()
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
            {
                 return Unauthorized(ApiResponse<object>.FailureResponse("User does not belong to a company.", 401));
            }

            try
            {
                var customers = await _customerService.GetCustomersAsync(tenantId, companyId);
                return Ok(ApiResponse<IEnumerable<CustomerDto>>.SuccessResponse(customers, "Customers retrieved."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.FailureResponse(ex.Message, 500));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CustomerDto>>> GetCustomer(Guid id)
        {
             var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
             try
             {
                 var customer = await _customerService.GetCustomerByIdAsync(id, tenantId);
                 return Ok(ApiResponse<CustomerDto>.SuccessResponse(customer, "Customer retrieved."));
             }
             catch (KeyNotFoundException ex)
             {
                 return NotFound(ApiResponse<object>.FailureResponse(ex.Message, 404));
             }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<CustomerDto>>> CreateCustomer([FromBody] CreateCustomerDto request)
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
                var customer = await _customerService.CreateCustomerAsync(request, tenantId, userId, companyId);
                return StatusCode(201, ApiResponse<CustomerDto>.SuccessResponse(customer, "Customer created successfully."));
            }
            catch (InvalidOperationException ex)
            {
                 return BadRequest(ApiResponse<object>.FailureResponse(ex.Message, 400));
            }
            catch (Exception ex)
            {
                 return StatusCode(500, ApiResponse<object>.FailureResponse(ex.Message, 500));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<CustomerDto>>> UpdateCustomer(Guid id, [FromBody] UpdateCustomerDto request)
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
                var customer = await _customerService.UpdateCustomerAsync(id, request, tenantId, userId);
                return Ok(ApiResponse<CustomerDto>.SuccessResponse(customer, "Customer updated successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                 return NotFound(ApiResponse<object>.FailureResponse(ex.Message, 404));
            }
            catch (Exception ex)
            {
                 return StatusCode(500, ApiResponse<object>.FailureResponse(ex.Message, 500));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteCustomer(Guid id)
        {
             var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
             var deleted = await _customerService.DeleteCustomerAsync(id, tenantId);
             
             if (!deleted)
             {
                 return NotFound(ApiResponse<object>.FailureResponse("Customer not found.", 404));
             }

             return Ok(ApiResponse<object>.SuccessResponse(null, "Customer deleted."));
        }
    }
}
