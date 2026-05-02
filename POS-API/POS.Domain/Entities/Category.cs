using POS.Domain.Common;

namespace POS.Domain.Entities
{
    public class Category : BaseEntity
    {
        public Guid CompanyId { get; set; }
        public Guid TenantId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public Guid? ParentCategoryId { get; set; }
        public Guid? LocationId { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid? CreatedBy { get; set; }

        public Company? Company { get; set; }
        public Category? ParentCategory { get; set; }
        public Location? Location { get; set; }
        
        public ICollection<Category> SubCategories { get; set; } = new List<Category>();
    }
}
