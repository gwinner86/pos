using System.ComponentModel.DataAnnotations;

namespace POS.Application.DTOs.Location
{
    public class CreateLocationDto
    {
        [Required]
        public string LocationName { get; set; } = string.Empty;
        


        public int? FeatureId { get; set; } // Optional as per entity definition? Or required? Entity has int? 
        
        public string LocationType { get; set; } = string.Empty; // e.g. Warehouse, Store

        public string? AddressLine1 { get; set; }
        public Guid? CurrencyId { get; set; }
        public int VatCalculationType { get; set; } = 1;
    }

    public class UpdateLocationDto
    {
        [Required]
        public string LocationName { get; set; } = string.Empty;
        
        public string LocationType { get; set; } = string.Empty;

        public string? AddressLine1 { get; set; }
        public Guid? CurrencyId { get; set; }
        public int VatCalculationType { get; set; } = 1;
    }

    public class LocationResponse
    {
        public Guid Id { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public Guid TenantId { get; set; }
        public Guid CompanyId { get; set; }
        public int? FeatureId { get; set; }
        public string LocationType { get; set; } = string.Empty;
        public string? AddressLine1 { get; set; }
        public Guid? CurrencyId { get; set; }
        public string? CurrencySymbol { get; set; }
        public string? CurrencyCode { get; set; }
        public int VatCalculationType { get; set; }
    }
}
