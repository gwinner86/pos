namespace POS.Application.DTOs.Auth
{
    public class RegisterTenantRequest
    {
        public string TenantName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int FeatureId { get; set; }
        public string CompanyPrimaryPhoneNumber { get; set; } = string.Empty;
        public string? CompanyEmailAddress { get; set; }
        public string LocationName { get; set; } = string.Empty;
    }
}
