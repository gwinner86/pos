namespace POS.Application.DTOs.Tenant
{
    public class TenantDto
    {
        public Guid Id { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public int FeatureId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateTenantDto
    {
        public string TenantName { get; set; } = string.Empty;
        public int FeatureId { get; set; }
    }

    public class UpdateTenantDto
    {
        public string TenantName { get; set; } = string.Empty;
        public int FeatureId { get; set; }
        public bool IsActive { get; set; }
    }
}
