using System.ComponentModel.DataAnnotations;

namespace POS.Application.DTOs.Product
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSkuBase { get; set; } = string.Empty;
        public Guid? CategoryId { get; set; }
        public string? CategoryName { get; set; } // Flattened for convenience
        public Guid? SupplierId { get; set; } // ADDED
        public string? SupplierName { get; set; } // ADDED
        public Guid? LocationId { get; set; }
        public string? LocationName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public decimal Price { get; set; }
        public decimal Cost { get; set; }
        public int StockLevel { get; set; }
        public int NewStock { get; set; }
        public int MinStockLevel { get; set; }
        public bool IsVatExcluded { get; set; }
        public string? Image1 { get; set; }
        public string? Image2 { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        public ICollection<ProductVariantDto> Variants { get; set; } = new List<ProductVariantDto>();
    }

    public class ProductVariantDto
    {
        public Guid Id { get; set; }
        public string VariantName { get; set; } = string.Empty;
        public string VariantSku { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public decimal Price { get; set; }
        public decimal Cost { get; set; }
        public int StockLevel { get; set; }
        public int MinStockLevel { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateProductDto
    {
        public string ProductName { get; set; } = string.Empty;
        public string ProductSkuBase { get; set; } = string.Empty;
        public Guid? CategoryId { get; set; }
        public Guid? LocationId { get; set; }
        public Guid? SupplierId { get; set; } // Added for Cost creation
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public decimal? Price { get; set; }
        public decimal? Cost { get; set; }
        public int? StockLevel { get; set; }
        public int? NewStock { get; set; }
        public int? MinStockLevel { get; set; }
        public bool IsVatExcluded { get; set; } = false;
        public string? Image1 { get; set; }
        public string? Image2 { get; set; }
        
        public ICollection<CreateProductVariantDto> Variants { get; set; } = new List<CreateProductVariantDto>();
    }

    public class CreateProductVariantDto
    {
        public string VariantName { get; set; } = string.Empty;
        public string VariantSku { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public decimal? Price { get; set; }
        public decimal? Cost { get; set; }
        public int? StockLevel { get; set; }
        public int? MinStockLevel { get; set; }
    }

    public class UpdateProductDto
    {
        public string ProductName { get; set; } = string.Empty;
        public string ProductSkuBase { get; set; } = string.Empty;
        public Guid? CategoryId { get; set; }
        public Guid? LocationId { get; set; }
        public Guid? SupplierId { get; set; } // Added for Cost update
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public decimal? Price { get; set; }
        public decimal? Cost { get; set; }
        public int? StockLevel { get; set; }
        public int? NewStock { get; set; }
        public int? MinStockLevel { get; set; }
        public bool IsVatExcluded { get; set; }
        public string? Image1 { get; set; }
        public string? Image2 { get; set; }

        // REQUIRED: Reason for update
        public string UpdateReason { get; set; } = string.Empty;
        
        public ICollection<UpdateProductVariantDto>? Variants { get; set; }
    }

    public class UpdateProductVariantDto
    {
        public Guid? Id { get; set; } // Null = New Variant
        public string VariantName { get; set; } = string.Empty;
        public string VariantSku { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public decimal? Price { get; set; }
        public decimal? Cost { get; set; }
        public int? StockLevel { get; set; }
        public int? MinStockLevel { get; set; }
    }
}
