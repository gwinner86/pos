using System.ComponentModel.DataAnnotations;

namespace POS.Application.DTOs.Currency
{
    public class CurrencyDto
    {
        public Guid Id { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public string CurrencySymbol { get; set; } = string.Empty;
        public string CurrencyName { get; set; } = string.Empty;
    }

    public class SetCurrencyDto
    {
        [Required]
        [StringLength(3)]
        public string CurrencyCode { get; set; } = string.Empty;

        [Required]
        public string CurrencySymbol { get; set; } = string.Empty;

        [Required]
        public string CurrencyName { get; set; } = string.Empty;
    }
}
