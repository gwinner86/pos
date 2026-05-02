namespace POS.Application.DTOs.Permission
{
    public class PermissionDto
    {
        public int PermissionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Group { get; set; }
    }
}
