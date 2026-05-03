using System.ComponentModel.DataAnnotations;

namespace POS.Application.DTOs.Supplier
{
    public class SupplierDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string? ContactName { get; set; }
        public string? ContactEmail { get; set; }
        public string? Phone { get; set; }
        public string? Terms { get; set; }
        public bool IsActive { get; set; }
        public Guid? LocationId { get; set; }
        public string? LocationName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateSupplierDto
    {
        public string SupplierName { get; set; } = string.Empty;
        public string? ContactName { get; set; }
        public string? ContactEmail { get; set; }
        public string? Phone { get; set; }
        public string? Terms { get; set; }
        public Guid? LocationId { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateSupplierDto
    {
        public string SupplierName { get; set; } = string.Empty;
        public string? ContactName { get; set; }
        public string? ContactEmail { get; set; }
        public string? Phone { get; set; }
        public string? Terms { get; set; }
        public Guid? LocationId { get; set; }
        public bool IsActive { get; set; }
    }
}
