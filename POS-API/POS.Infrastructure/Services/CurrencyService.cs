using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs.Currency;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Infrastructure.Persistence;

namespace POS.Infrastructure.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly ApplicationDbContext _context;

        public CurrencyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CurrencyDto>> GetCurrenciesAsync(Guid companyId)
        {
            var currencies = await _context.Currencies
                .Where(c => c.CompanyId == companyId)
                .ToListAsync();

            return currencies.Select(MapToDto);
        }

        public async Task<CurrencyDto> CreateCurrencyAsync(SetCurrencyDto request, Guid companyId, Guid userId)
        {
            // Allow multiple currencies, so no check for existing
            var currency = new Currency
            {
                CompanyId = companyId,
                CurrencyCode = request.CurrencyCode,
                CurrencyName = request.CurrencyName,
                CurrencySymbol = request.CurrencySymbol,
                CreatedBy = userId,
                IsActive = true
            };

            // Resolve TenantId from Company
            var company = await _context.Companies.FindAsync(companyId);
            if (company != null) currency.TenantId = company.TenantId;

            _context.Currencies.Add(currency);
            await _context.SaveChangesAsync();

            return MapToDto(currency);
        }

        public async Task<CurrencyDto> UpdateCurrencyAsync(Guid currencyId, SetCurrencyDto request, Guid companyId, Guid userId)
        {
             var currency = await _context.Currencies
                .FirstOrDefaultAsync(c => c.Id == currencyId && c.CompanyId == companyId);

            if (currency == null)
                throw new KeyNotFoundException("Currency not found.");

            currency.CurrencyCode = request.CurrencyCode;
            currency.CurrencyName = request.CurrencyName;
            currency.CurrencySymbol = request.CurrencySymbol;
            currency.UpdatedBy = userId;
            currency.LastUpdated = DateTime.UtcNow;
                
            _context.Currencies.Update(currency);
            await _context.SaveChangesAsync();

            return MapToDto(currency);
        }

        private static CurrencyDto MapToDto(Currency currency)
        {
            return new CurrencyDto
            {
                Id = currency.Id,
                CurrencyCode = currency.CurrencyCode,
                CurrencyName = currency.CurrencyName,
                CurrencySymbol = currency.CurrencySymbol
            };
        }
    }
}
