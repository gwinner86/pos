using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Common.Models;
using POS.Application.DTOs.Category;
using POS.Application.DTOs.Category.Validators;
using POS.Application.Interfaces;
using FluentValidation;

namespace POS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly IValidator<CreateCategoryDto> _createValidator;

        public CategoriesController(ICategoryService categoryService, IValidator<CreateCategoryDto> createValidator)
        {
            _categoryService = categoryService;
            _createValidator = createValidator;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> GetAllCategories()
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
            var categories = await _categoryService.GetAllCategoriesAsync(tenantId);
            return Ok(ApiResponse<IEnumerable<CategoryDto>>.SuccessResponse(categories, "Categories retrieved successfully."));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> GetCategoryById(Guid id)
        {
            try 
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                return Ok(ApiResponse<CategoryDto>.SuccessResponse(category, "Category retrieved successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.FailureResponse(ex.Message, 404));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> CreateCategory([FromBody] CreateCategoryDto request)
        {
            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                 var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                 return BadRequest(ApiResponse<object>.FailureResponse(errors, 400));
            }

            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
            
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim))
            {
                return Unauthorized(ApiResponse<object>.FailureResponse("User does not belong to a company.", 401));
            }
            var companyId = Guid.Parse(companyIdClaim);

            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);

            try
            {
                var category = await _categoryService.CreateCategoryAsync(request, tenantId, userId, companyId);
                return CreatedAtAction(nameof(GetCategoryById), new { id = category.CategoryId }, ApiResponse<CategoryDto>.SuccessResponse(category, "Category created successfully."));
            }
            catch (Exception ex)
            {
                 return StatusCode(500, ApiResponse<object>.FailureResponse($"An error occurred: {ex.Message}", 500));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> UpdateCategory(Guid id, [FromBody] UpdateCategoryDto request)
        {
             var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);

             try
             {
                 var category = await _categoryService.UpdateCategoryAsync(id, request, userId);
                 return Ok(ApiResponse<CategoryDto>.SuccessResponse(category, "Category updated successfully."));
             }
             catch (KeyNotFoundException ex)
             {
                 return NotFound(ApiResponse<object>.FailureResponse(ex.Message, 404));
             }
             catch (Exception ex)
             {
                 return StatusCode(500, ApiResponse<object>.FailureResponse($"An error occurred: {ex.Message}", 500));
             }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteCategory(Guid id)
        {
            try
            {
                await _categoryService.DeleteCategoryAsync(id);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Category deleted successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.FailureResponse(ex.Message, 404));
            }
            catch (Exception ex) // Catch duplicate or constraint checks
            {
                return StatusCode(500, ApiResponse<object>.FailureResponse($"An error occurred: {ex.Message}", 500));
            }
        }
    }
}
