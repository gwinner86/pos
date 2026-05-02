using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Common.Models;
using POS.Application.DTOs.Product;
using POS.Application.Interfaces;

namespace POS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IValidator<CreateProductDto> _createValidator;
        private readonly IValidator<UpdateProductDto> _updateValidator;

        public ProductsController(
            IProductService productService, 
            IValidator<CreateProductDto> createValidator,
            IValidator<UpdateProductDto> updateValidator)
        {
            _productService = productService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetProducts([FromQuery] Guid? locationId = null)
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);

            // Prefer the query param (reflects the user's currently selected branch in the UI).
            // Fall back to the JWT claim only when no query param is provided.
            if (!locationId.HasValue)
            {
                var locationIdClaim = User.FindFirst("LocationId")?.Value;
                if (!string.IsNullOrEmpty(locationIdClaim) && Guid.TryParse(locationIdClaim, out var claimLocId))
                    locationId = claimLocId;
            }

            var products = await _productService.GetProductsAsync(tenantId, locationId);
            return Ok(ApiResponse<IEnumerable<ProductDto>>.SuccessResponse(products, "Products retrieved."));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ProductDto>>> GetProduct(Guid id, [FromQuery] Guid? locationId = null)
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);

            if (!locationId.HasValue)
            {
                var locationIdClaim = User.FindFirst("LocationId")?.Value;
                if (!string.IsNullOrEmpty(locationIdClaim) && Guid.TryParse(locationIdClaim, out var claimLocId))
                    locationId = claimLocId;
            }

            try
            {
                var product = await _productService.GetProductByIdAsync(id, tenantId, locationId);
                return Ok(ApiResponse<ProductDto>.SuccessResponse(product, "Product retrieved."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.FailureResponse(ex.Message, 404));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ProductDto>>> CreateProduct([FromBody] CreateProductDto request)
        {
            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return BadRequest(ApiResponse<object>.FailureResponse(errors, 400));
            }

            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
            {
                return Unauthorized(ApiResponse<object>.FailureResponse("User does not belong to a company.", 401));
            }
            
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);

            try
            {
                var product = await _productService.CreateProductAsync(request, tenantId, userId, companyId);
                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, ApiResponse<ProductDto>.SuccessResponse(product, "Product created."));
            }
            catch (InvalidOperationException ex) // SKU conflict
            {
                return Conflict(ApiResponse<object>.FailureResponse(ex.Message, 409));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.FailureResponse(ex.Message, 500));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<ProductDto>>> UpdateProduct(Guid id, [FromBody] UpdateProductDto request)
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
                var product = await _productService.UpdateProductAsync(id, request, tenantId, userId);
                return Ok(ApiResponse<ProductDto>.SuccessResponse(product, "Product updated."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.FailureResponse(ex.Message, 404));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<object>.FailureResponse(ex.Message, 409));
            }
            catch (Exception ex)
            {
                 // Return the INNER exception message to see SQL errors
                 var message = ex.InnerException?.Message ?? ex.Message;
                 return StatusCode(500, ApiResponse<object>.FailureResponse($"Error: {message}", 500));
            }
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteProduct(Guid id)
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);

            try
            {
                await _productService.DeleteProductAsync(id, tenantId, userId);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Product deleted (deactivated)."));
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

        [HttpDelete("all")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteAllProducts()
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);

            try
            {
                var count = await _productService.DeleteAllProductsAsync(tenantId, userId);
                return Ok(ApiResponse<object>.SuccessResponse(new { DeletedCount = count }, $"{count} product(s) permanently deleted."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.FailureResponse(ex.Message, 500));
            }
        }


        [HttpPost("bulk-upload")]
        public async Task<ActionResult<ApiResponse<object>>> BulkUploadProducts(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<object>.FailureResponse("No file uploaded.", 400));

            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;

            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
            {
                return Unauthorized(ApiResponse<object>.FailureResponse("User does not belong to a company.", 401));
            }

            try
            {
                using var stream = file.OpenReadStream();
                var count = await _productService.BulkUploadProductsAsync(stream, file.FileName, tenantId, userId, companyId);
                return Ok(ApiResponse<object>.SuccessResponse(new { ImportedCount = count }, $"Successfully imported {count} products."));
            }
            catch (Exception ex)
            {
                var message = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, ApiResponse<object>.FailureResponse($"Error processing bulk upload: {message}", 500));
            }
        }
        [HttpGet("{id}/locations")]
        public async Task<ActionResult<ApiResponse<IEnumerable<Guid>>>> GetProductLocations(Guid id)
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
            try
            {
                var locationIds = await _productService.GetProductLocationAssignmentsAsync(id, tenantId);
                return Ok(ApiResponse<IEnumerable<Guid>>.SuccessResponse(locationIds, "Product branch assignments retrieved."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.FailureResponse(ex.Message, 404));
            }
        }

        [HttpPut("{id}/locations")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateProductLocations(Guid id, [FromBody] List<Guid> locationIds)
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;

            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
                return Unauthorized(ApiResponse<object>.FailureResponse("User does not belong to a company.", 401));

            try
            {
                await _productService.UpdateProductLocationAssignmentsAsync(id, locationIds, tenantId, userId, companyId);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Branch assignments updated successfully."));
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
    }
}
