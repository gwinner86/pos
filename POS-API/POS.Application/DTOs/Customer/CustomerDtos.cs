using System.ComponentModel.DataAnnotations;

namespace POS.Application.DTOs.Customer
{
    public class CustomerDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public string? CompanyName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? AddressLine1 { get; set; }
        public string? City { get; set; }
        public decimal LoyaltyPoints { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCustomerDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public string? CompanyName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? AddressLine1 { get; set; }
        public string? City { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
    }

    public class UpdateCustomerDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? CompanyName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? AddressLine1 { get; set; }
        public string? City { get; set; }
        // Loyalty points usually updated by system logic, but maybe manual override allowed?
        // I'll leave it out of standard update for now to prevent accidental overrides, unless needed.
    }
}
