namespace POS.Application.DTOs.Category
{
    public class CreateCategoryDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public Guid? ParentCategoryId { get; set; }
        public Guid? LocationId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
