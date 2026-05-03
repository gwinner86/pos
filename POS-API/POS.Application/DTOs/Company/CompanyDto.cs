using System;

namespace POS.Application.DTOs.Company
{
    public class CompanyDto
    {
        public Guid CompanyId { get; set; }
        public Guid TenantId { get; set; }
        public int? FeatureId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyAddress { get; set; }
        public string? CompanyPrimaryPhoneNumber { get; set; }
        public string? CompanyOtherPhoneNumbers { get; set; }
        public string? CompanyEmail { get; set; }
        public string? TaxId { get; set; }
        public bool IsActive { get; set; }
    }
}
