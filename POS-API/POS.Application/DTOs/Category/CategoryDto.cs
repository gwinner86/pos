namespace POS.Application.DTOs.Category
{
    public class CategoryDto
    {
        public Guid CategoryId { get; set; }
        public Guid CompanyId { get; set; }
        public Guid TenantId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public Guid? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; } // Flattened
        public Guid? LocationId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Optional: Include children recursively?
        // For simplicity in list views, maybe not.
        public List<CategoryDto> SubCategories { get; set; } = new List<CategoryDto>();
    }
}
