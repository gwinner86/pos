using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs.Company;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace POS.Infrastructure.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly ApplicationDbContext _context;

        public CompanyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<CompanyDto>> GetAllCompaniesAsync()
        {
            var companies = await _context.Companies
                .AsNoTracking()
                .ToListAsync();

            return companies.Select(MapToDto).ToList();
        }

        public async Task<CompanyDto?> GetCompanyByIdAsync(Guid id)
        {
            var company = await _context.Companies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (company == null) return null;

            return MapToDto(company);
        }

        public async Task<IReadOnlyList<CompanyDto>> GetCompaniesByTenantIdAsync(Guid tenantId)
        {
            var companies = await _context.Companies
                .Where(c => c.TenantId == tenantId)
                .AsNoTracking()
                .ToListAsync();

            return companies.Select(MapToDto).ToList();
        }

        public async Task<CompanyDto> CreateCompanyAsync(CreateCompanyDto createCompanyDto)
        {
            var company = new Company
            {
                Id = Guid.NewGuid(),
                TenantId = createCompanyDto.TenantId,
                FeatureId = createCompanyDto.FeatureId,
                CompanyName = createCompanyDto.CompanyName,
                CompanyAddress = createCompanyDto.CompanyAddress,
                CompanyPrimaryPhoneNumber = createCompanyDto.CompanyPrimaryPhoneNumber,
                CompanyOtherPhoneNumbers = createCompanyDto.CompanyOtherPhoneNumbers,
                CompanyEmail = createCompanyDto.CompanyEmail,
                TaxId = createCompanyDto.TaxId,
                IsActive = createCompanyDto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            return MapToDto(company);
        }

        public async Task UpdateCompanyAsync(Guid id, UpdateCompanyDto updateCompanyDto)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                throw new KeyNotFoundException($"Company with ID {id} not found.");
            }

            company.FeatureId = updateCompanyDto.FeatureId;
            company.CompanyName = updateCompanyDto.CompanyName;
            company.CompanyAddress = updateCompanyDto.CompanyAddress;
            company.CompanyPrimaryPhoneNumber = updateCompanyDto.CompanyPrimaryPhoneNumber;
            company.CompanyOtherPhoneNumbers = updateCompanyDto.CompanyOtherPhoneNumbers;
            company.CompanyEmail = updateCompanyDto.CompanyEmail;
            company.TaxId = updateCompanyDto.TaxId;
            company.IsActive = updateCompanyDto.IsActive;
            company.UpdatedAt = DateTime.UtcNow;

            _context.Companies.Update(company);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCompanyAsync(Guid id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company != null)
            {
                _context.Companies.Remove(company);
                await _context.SaveChangesAsync();
            }
        }

        private static CompanyDto MapToDto(Company company)
        {
            return new CompanyDto
            {
                CompanyId = company.Id,
                TenantId = company.TenantId,
                FeatureId = company.FeatureId,
                CompanyName = company.CompanyName,
                CompanyAddress = company.CompanyAddress,
                CompanyPrimaryPhoneNumber = company.CompanyPrimaryPhoneNumber,
                CompanyOtherPhoneNumbers = company.CompanyOtherPhoneNumbers,
                CompanyEmail = company.CompanyEmail,
                TaxId = company.TaxId,
                IsActive = company.IsActive
            };
        }
    }
}
