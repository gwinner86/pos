using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs.Settings;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Infrastructure.Persistence;

namespace POS.Infrastructure.Services
{
    public class SetupCountryService : ISetupCountryService
    {
        private readonly ApplicationDbContext _context;

        public SetupCountryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SetupCountryDto>> GetAllSetupCountriesAsync(Guid companyId)
        {
            var countries = await _context.SetupCountries
                .Where(c => c.CompanyId == companyId)
                .AsNoTracking()
                .ToListAsync();

            return countries.Select(c => new SetupCountryDto
            {
                Id = c.Id,
                TenantId = c.TenantId,
                CompanyId = c.CompanyId,
                LocationId = c.LocationId,
                CountryName = c.CountryName,
                CountryCode = c.CountryCode,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            });
        }

        public async Task<SetupCountryDto?> GetSetupCountryByIdAsync(Guid id)
        {
            var c = await _context.SetupCountries.FindAsync(id);
            if (c == null) return null;

            return new SetupCountryDto
            {
                Id = c.Id,
                TenantId = c.TenantId,
                CompanyId = c.CompanyId,
                LocationId = c.LocationId,
                CountryName = c.CountryName,
                CountryCode = c.CountryCode,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            };
        }

        public async Task<SetupCountryDto> CreateSetupCountryAsync(CreateSetupCountryDto request)
        {
            var country = new SetupCountry
            {
                TenantId = request.TenantId,
                CompanyId = request.CompanyId,
                LocationId = request.LocationId,
                CountryName = request.CountryName,
                CountryCode = request.CountryCode,
                IsActive = true
            };

            _context.SetupCountries.Add(country);
            await _context.SaveChangesAsync();

            return new SetupCountryDto
            {
                Id = country.Id,
                TenantId = country.TenantId,
                CompanyId = country.CompanyId,
                LocationId = country.LocationId,
                CountryName = country.CountryName,
                CountryCode = country.CountryCode,
                IsActive = country.IsActive,
                CreatedAt = country.CreatedAt,
                UpdatedAt = country.UpdatedAt
            };
        }

        public async Task<SetupCountryDto> UpdateSetupCountryAsync(Guid id, UpdateSetupCountryDto request)
        {
            var country = await _context.SetupCountries.FindAsync(id);
            if (country == null) throw new KeyNotFoundException("SetupCountry not found");

            country.CountryName = request.CountryName;
            country.CountryCode = request.CountryCode;
            country.IsActive = request.IsActive;

            _context.SetupCountries.Update(country);
            await _context.SaveChangesAsync();

            return new SetupCountryDto
            {
                Id = country.Id,
                TenantId = country.TenantId,
                CompanyId = country.CompanyId,
                LocationId = country.LocationId,
                CountryName = country.CountryName,
                CountryCode = country.CountryCode,
                IsActive = country.IsActive,
                CreatedAt = country.CreatedAt,
                UpdatedAt = country.UpdatedAt
            };
        }

        public async Task<bool> DeleteSetupCountryAsync(Guid id)
        {
            var country = await _context.SetupCountries.FindAsync(id);
            if (country == null) return false;

            _context.SetupCountries.Remove(country);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
