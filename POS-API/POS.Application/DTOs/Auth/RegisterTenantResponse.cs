namespace POS.Application.DTOs.Auth
{
    public class RegisterTenantResponse
    {
        public Guid UserId { get; set; }
        public Guid TenantId { get; set; }
        public Guid CompanyId { get; set; }
    }
}
