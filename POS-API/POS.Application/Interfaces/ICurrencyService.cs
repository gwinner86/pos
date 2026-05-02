using POS.Application.DTOs.Currency;

namespace POS.Application.Interfaces
{
    public interface ICurrencyService
    {
        Task<IEnumerable<CurrencyDto>> GetCurrenciesAsync(Guid companyId);
        Task<CurrencyDto> CreateCurrencyAsync(SetCurrencyDto request, Guid companyId, Guid userId);
        Task<CurrencyDto> UpdateCurrencyAsync(Guid currencyId, SetCurrencyDto request, Guid companyId, Guid userId);
    }
}
