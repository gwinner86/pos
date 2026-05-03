using Microsoft.AspNetCore.Mvc;
using POS.Application.Common.Models;
using POS.Application.DTOs.Features;
using POS.Application.Interfaces;

namespace POS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeaturesController : ControllerBase
    {
        private readonly IFeatureService _featureService;
        private readonly FluentValidation.IValidator<CreateFeatureDto> _validator;

        public FeaturesController(IFeatureService featureService, FluentValidation.IValidator<CreateFeatureDto> validator)
        {
            _featureService = featureService;
            _validator = validator;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<FeatureDto>>>> GetAll()
        {
            var features = await _featureService.GetAllFeaturesAsync();
            return Ok(ApiResponse<IReadOnlyList<FeatureDto>>.SuccessResponse(features, "Features retrieved successfully"));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<FeatureDto>>> GetById(int id)
        {
            var feature = await _featureService.GetFeatureByIdAsync(id);
            if (feature == null)
            {
                return NotFound(ApiResponse<FeatureDto>.FailureResponse($"Feature with ID {id} not found"));
            }
            return Ok(ApiResponse<FeatureDto>.SuccessResponse(feature, "Feature retrieved successfully"));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<string>>> Create([FromBody] CreateFeatureDto createFeatureDto)
        {
            var validationResult = await _validator.ValidateAsync(createFeatureDto);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return BadRequest(ApiResponse<string>.FailureResponse(errors));
            }

            await _featureService.CreateFeatureAsync(createFeatureDto);
            // Returning null data as requested
            return Ok(ApiResponse<string>.SuccessResponse(null, "Feature created successfully"));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<string>>> Update(int id, [FromBody] UpdateFeatureDto updateFeatureDto)
        {
            try
            {
                await _featureService.UpdateFeatureAsync(id, updateFeatureDto);
                return Ok(ApiResponse<string>.SuccessResponse(null, "Feature updated successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.FailureResponse(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<string>>> Delete(int id)
        {
            await _featureService.DeleteFeatureAsync(id);
            return Ok(ApiResponse<string>.SuccessResponse(null, "Feature deleted successfully"));
        }
    }
}
