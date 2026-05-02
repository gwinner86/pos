using System;

namespace POS.Application.DTOs.Settings
{
    public class SetupVATDto
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public Guid CompanyId { get; set; }
        public Guid? LocationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public string? Description { get; set; }
        public string? CountryCode { get; set; }
        public Guid? MakerId { get; set; }
        public DateTime? MakeDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateSetupVATDto
    {
        public Guid CompanyId { get; set; }
        public Guid? LocationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public string? Description { get; set; }
        public string? CountryCode { get; set; }
        public Guid? MakerId { get; set; }
        public DateTime? MakeDate { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateSetupVATDto
    {
        public Guid CompanyId { get; set; }
        public Guid? LocationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public string? Description { get; set; }
        public string? CountryCode { get; set; }
        public bool IsActive { get; set; }
    }
}
