using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using POS.Application.DTOs.Company;

namespace POS.Application.Interfaces
{
    public interface ICompanyService
    {
        Task<IReadOnlyList<CompanyDto>> GetAllCompaniesAsync();
        Task<CompanyDto?> GetCompanyByIdAsync(Guid id);
        Task<IReadOnlyList<CompanyDto>> GetCompaniesByTenantIdAsync(Guid tenantId);
        Task<CompanyDto> CreateCompanyAsync(CreateCompanyDto createCompanyDto);
        Task UpdateCompanyAsync(Guid id, UpdateCompanyDto updateCompanyDto);
        Task DeleteCompanyAsync(Guid id);
    }
}
