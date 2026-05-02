using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs.Category;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Infrastructure.Persistence;

namespace POS.Infrastructure.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(Guid tenantId)
        {
            return await _context.Categories
                .Where(c => c.TenantId == tenantId && c.IsActive)
                .Include(c => c.ParentCategory)
                .OrderBy(c => c.CategoryName)
                .Select(c => new CategoryDto
                {
                    CategoryId = c.Id,
                    CompanyId = c.CompanyId,
                    TenantId = c.TenantId,
                    CategoryName = c.CategoryName,
                    ParentCategoryId = c.ParentCategoryId,
                    ParentCategoryName = c.ParentCategory != null ? c.ParentCategory.CategoryName : null,
                    LocationId = c.LocationId,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<CategoryDto> GetCategoryByIdAsync(Guid id)
        {
            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) throw new KeyNotFoundException($"Category with ID {id} not found.");

            return new CategoryDto
            {
                CategoryId = category.Id,
                CompanyId = category.CompanyId,
                TenantId = category.TenantId,
                CategoryName = category.CategoryName,
                ParentCategoryId = category.ParentCategoryId,
                ParentCategoryName = category.ParentCategory?.CategoryName,
                LocationId = category.LocationId,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt
            };
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto, Guid tenantId, Guid userId, Guid companyId)
        {
            // Validate Parent Category if provided
            if (dto.ParentCategoryId.HasValue)
            {
                var parentExists = await _context.Categories.AnyAsync(c => c.Id == dto.ParentCategoryId && c.TenantId == tenantId);
                if (!parentExists) throw new ArgumentException("Invalid Parent Category ID.");
            }

            var category = new Category
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CompanyId = companyId,
                CategoryName = dto.CategoryName,
                ParentCategoryId = dto.ParentCategoryId,
                LocationId = dto.LocationId,
                IsActive = dto.IsActive,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return await GetCategoryByIdAsync(category.Id);
        }

        public async Task<CategoryDto> UpdateCategoryAsync(Guid id, UpdateCategoryDto dto, Guid userId)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) throw new KeyNotFoundException($"Category with ID {id} not found.");

            // Loop detection check (prevent setting parent to itself or its own child is complex, 
            // but ensuring parent != id is a good basic check)
            if (dto.ParentCategoryId.HasValue && dto.ParentCategoryId == id)
            {
                 throw new ArgumentException("A category cannot be its own parent.");
            }

            category.CategoryName = dto.CategoryName;
            category.ParentCategoryId = dto.ParentCategoryId;
            category.LocationId = dto.LocationId;
            category.IsActive = dto.IsActive;
            category.UpdatedAt = DateTime.UtcNow;
            // category.UpdatedBy = userId; // If BaseEntity or Category has UpdatedBy

            await _context.SaveChangesAsync();

            return await GetCategoryByIdAsync(id);
        }

        public async Task DeleteCategoryAsync(Guid id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) throw new KeyNotFoundException($"Category with ID {id} not found.");

            // Soft Delete or Hard Delete? usually Soft Delete if IsActive is present
            // But user asked for Delete impl. Let's do Soft Delete if it's strictly enforced, 
            // or just Remove if they want deletion. 
            // Given 'IsActive' on entity, typically we toggle that. 
            // BUT standard CRUD DELETE usually means removal or disabling.
            // Let's implement Hard Delete but check for dependencies (Products)
            
            var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
            if (hasProducts)
                throw new InvalidOperationException("Cannot delete category with associated products.");

            var hasSubCategories = await _context.Categories.AnyAsync(c => c.ParentCategoryId == id);
            if (hasSubCategories)
                throw new InvalidOperationException("Cannot delete category with sub-categories.");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }
}
