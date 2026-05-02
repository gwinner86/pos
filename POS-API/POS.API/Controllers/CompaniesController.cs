using Microsoft.AspNetCore.Mvc;
using POS.Application.Common.Models;
using POS.Application.DTOs.Company;
using POS.Application.Interfaces;
using FluentValidation;

namespace POS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyService _companyService;
        private readonly IValidator<CreateCompanyDto> _validator;

        public CompaniesController(ICompanyService companyService, IValidator<CreateCompanyDto> validator)
        {
            _companyService = companyService;
            _validator = validator;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<CompanyDto>>>> GetAll()
        {
            var companies = await _companyService.GetAllCompaniesAsync();
            return Ok(ApiResponse<IReadOnlyList<CompanyDto>>.SuccessResponse(companies, "Companies retrieved successfully"));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CompanyDto>>> GetById(Guid id)
        {
            var company = await _companyService.GetCompanyByIdAsync(id);
            if (company == null)
            {
                return NotFound(ApiResponse<CompanyDto>.FailureResponse($"Company with ID {id} not found"));
            }
            return Ok(ApiResponse<CompanyDto>.SuccessResponse(company, "Company retrieved successfully"));
        }

        [HttpGet("tenant/{tenantId}")]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<CompanyDto>>>> GetByTenantId(Guid tenantId)
        {
            var companies = await _companyService.GetCompaniesByTenantIdAsync(tenantId);
            return Ok(ApiResponse<IReadOnlyList<CompanyDto>>.SuccessResponse(companies, "Companies retrieved successfully"));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<string>>> Create([FromBody] CreateCompanyDto createCompanyDto)
        {
            var validationResult = await _validator.ValidateAsync(createCompanyDto);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return BadRequest(ApiResponse<string>.FailureResponse(errors));
            }

            await _companyService.CreateCompanyAsync(createCompanyDto);
            // Returning null data as requested
            return Ok(ApiResponse<string>.SuccessResponse(null, "Company created successfully"));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<string>>> Update(Guid id, [FromBody] UpdateCompanyDto updateCompanyDto)
        {
            try
            {
                await _companyService.UpdateCompanyAsync(id, updateCompanyDto);
                return Ok(ApiResponse<string>.SuccessResponse(null, "Company updated successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.FailureResponse(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<string>>> Delete(Guid id)
        {
            await _companyService.DeleteCompanyAsync(id);
            return Ok(ApiResponse<string>.SuccessResponse(null, "Company deleted successfully"));
        }
    }
}
