using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs.Product;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Infrastructure.Persistence;
using ExcelDataReader;

namespace POS.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductDto>> GetProductsAsync(Guid tenantId, Guid? locationId = null)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Location) // Origin location
                .Include(p => p.Supplier) // Include Supplier
                .Include(p => p.Variants) // Base variants
                .Where(p => p.TenantId == tenantId)
                .AsSplitQuery(); // Optimize for collection includes

            var products = await query.ToListAsync();
            
            Dictionary<Guid, Inventory> inventories = new();
            Dictionary<Guid, Pricing> pricings = new();
            Dictionary<Guid, Cost> costs = new();
            Dictionary<Guid, decimal> globalInventory = new();

            if (locationId.HasValue)
            {
                var variantIds = products.SelectMany(p => p.Variants.Select(v => v.Id)).ToList();
                
                inventories = await _context.Inventory
                    .Where(i => variantIds.Contains(i.ProductVariantId) && i.LocationId == locationId.Value)
                    .ToDictionaryAsync(i => i.ProductVariantId);

                pricings = await _context.Pricing
                    .Where(p => p.ProductVariantId.HasValue && variantIds.Contains(p.ProductVariantId.Value) && p.LocationId == locationId.Value)
                    .ToDictionaryAsync(p => p.ProductVariantId!.Value);

                costs = await _context.Costs
                    .Where(c => variantIds.Contains(c.ProductVariantId) && c.LocationId == locationId.Value)
                    .ToDictionaryAsync(c => c.ProductVariantId);
            }
            else
            {
                var productIds = products.Select(p => p.Id).ToList();
                var globalInvQuery = await _context.Inventory
                    .Where(i => productIds.Contains(i.ProductId))
                    .GroupBy(i => i.ProductId)
                    .Select(g => new { ProductId = g.Key, TotalStock = g.Sum(i => i.Quantity) })
                    .ToListAsync();
                globalInventory = globalInvQuery.ToDictionary(x => x.ProductId, x => x.TotalStock);
            }

            return products.Select(p => {
                int pStock = 0;
                int pMinStock = 0;
                decimal pPrice = 0;
                decimal pCost = 0;

                if (!locationId.HasValue && globalInventory.TryGetValue(p.Id, out var gst)) {
                     pStock = (int)gst;
                }

                var variantsDto = p.Variants.Select(v => {
                    int vStock = 0;
                    int vMinStock = 0;
                    decimal vPrice = 0;
                    decimal vCost = 0;

                    if (locationId.HasValue) {
                        if (inventories.TryGetValue(v.Id, out var inv)) {
                            vStock = (int)inv.Quantity;
                            vMinStock = (int)inv.ReorderLevel;
                        }
                        if (pricings.TryGetValue(v.Id, out var prc)) vPrice = prc.Price;
                        if (costs.TryGetValue(v.Id, out var cst)) vCost = cst.CostValue;
                    }

                    return new ProductVariantDto {
                        Id = v.Id,
                        VariantName = v.VariantName,
                        VariantSku = v.VariantSku,
                        Barcode = v.Barcode,
                        Price = vPrice,
                        Cost = vCost,
                        StockLevel = vStock,
                        MinStockLevel = vMinStock,
                        CreatedAt = v.CreatedAt,
                        UpdatedAt = v.UpdatedAt ?? v.CreatedAt
                    };
                }).ToList();

                if (locationId.HasValue) {
                    pStock = variantsDto.Sum(v => v.StockLevel);
                    pMinStock = variantsDto.Sum(v => v.MinStockLevel);
                    if (variantsDto.Any()) {
                        pPrice = variantsDto.First().Price;
                        pCost = variantsDto.First().Cost;
                    }
                }

                return new ProductDto {
                    Id = p.Id,
                    ProductName = p.ProductName,
                    ProductSkuBase = p.ProductSkuBase,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category?.CategoryName,
                    LocationId = p.LocationId,
                    LocationName = p.Location?.LocationName,
                    Description = p.Description,
                    Price = pPrice,
                    Cost = pCost,
                    StockLevel = pStock,
                    NewStock = 0,
                    MinStockLevel = pMinStock,
                    Image1 = p.Image1,
                    IsActive = p.IsActive,
                    IsVatExcluded = p.IsVatExcluded,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt ?? p.CreatedAt,
                    SupplierId = p.SupplierId,
                    SupplierName = p.Supplier?.SupplierName,
                    Variants = variantsDto
                };
            }).ToList();
        }

        public async Task<ProductDto> GetProductByIdAsync(Guid productId, Guid tenantId, Guid? locationId = null)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Location) // Origin location
                .Include(p => p.Supplier) // Include Supplier
                .Include(p => p.Variants) // Base variants
                .FirstOrDefaultAsync(p => p.Id == productId && p.TenantId == tenantId);

            if (product == null) throw new KeyNotFoundException("Product not found.");

            int pStock = 0;
            int pMinStock = 0;
            decimal pPrice = 0;
            decimal pCost = 0;

            Dictionary<Guid, Inventory> inventories = new();
            Dictionary<Guid, Pricing> pricings = new();
            Dictionary<Guid, Cost> costs = new();

            if (locationId.HasValue)
            {
                var variantIds = product.Variants.Select(v => v.Id).ToList();

                inventories = await _context.Inventory
                    .Where(i => variantIds.Contains(i.ProductVariantId) && i.LocationId == locationId.Value)
                    .ToDictionaryAsync(i => i.ProductVariantId);

                pricings = await _context.Pricing
                    .Where(p => p.ProductVariantId.HasValue && variantIds.Contains(p.ProductVariantId.Value) && p.LocationId == locationId.Value)
                    .ToDictionaryAsync(p => p.ProductVariantId!.Value);

                costs = await _context.Costs
                    .Where(c => variantIds.Contains(c.ProductVariantId) && c.LocationId == locationId.Value)
                    .ToDictionaryAsync(c => c.ProductVariantId);
            }
            else
            {
                 var totalStock = await _context.Inventory
                     .Where(i => i.ProductId == product.Id)
                     .SumAsync(i => i.Quantity);
                 pStock = (int)totalStock;
            }

            var variantsDto = product.Variants.Select(v => {
                int vStock = 0;
                int vMinStock = 0;
                decimal vPrice = 0;
                decimal vCost = 0;

                if (locationId.HasValue) {
                    if (inventories.TryGetValue(v.Id, out var inv)) {
                        vStock = (int)inv.Quantity;
                        vMinStock = (int)inv.ReorderLevel;
                    }
                    if (pricings.TryGetValue(v.Id, out var prc)) vPrice = prc.Price;
                    if (costs.TryGetValue(v.Id, out var cst)) vCost = cst.CostValue;
                }

                return new ProductVariantDto {
                    Id = v.Id,
                    VariantName = v.VariantName,
                    VariantSku = v.VariantSku,
                    Barcode = v.Barcode,
                    Price = vPrice,
                    Cost = vCost,
                    StockLevel = vStock,
                    MinStockLevel = vMinStock,
                    CreatedAt = v.CreatedAt,
                    UpdatedAt = v.UpdatedAt ?? v.CreatedAt
                };
            }).ToList();

            if (locationId.HasValue) {
                pStock = variantsDto.Sum(v => v.StockLevel);
                pMinStock = variantsDto.Sum(v => v.MinStockLevel);
                if (variantsDto.Any()) {
                    pPrice = variantsDto.First().Price;
                    pCost = variantsDto.First().Cost;
                }
            }

            return new ProductDto
            {
                Id = product.Id,
                ProductName = product.ProductName,
                ProductSkuBase = product.ProductSkuBase,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.CategoryName,
                LocationId = product.LocationId,
                LocationName = product.Location?.LocationName,
                Description = product.Description,
                Price = pPrice,
                Cost = pCost,
                StockLevel = pStock,
                NewStock = 0,
                MinStockLevel = pMinStock,
                Image1 = product.Image1,
                IsActive = product.IsActive,
                IsVatExcluded = product.IsVatExcluded,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt ?? product.CreatedAt,
                SupplierId = product.SupplierId,
                SupplierName = product.Supplier?.SupplierName,
                Variants = variantsDto
            };
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto request, Guid tenantId, Guid userId, Guid companyId)
        {
            // Verify Company
            var companyExists = await _context.Companies.AnyAsync(c => c.Id == companyId && c.TenantId == tenantId);
            if (!companyExists) throw new KeyNotFoundException("Company not found.");

            // Verify SKU Uniqueness (scoped to Company?)
            var skuExists = await _context.Products.AnyAsync(p => p.ProductSkuBase == request.ProductSkuBase && p.CompanyId == companyId);
            if (skuExists) throw new InvalidOperationException($"Product with SKU '{request.ProductSkuBase}' already exists.");

            // Verify Variant SKUs Uniqueness
            // Note: This check is simple; in high concurrency, DB constraints are safer.
            foreach (var variantDto in request.Variants)
            {
               var variantSkuExists = await _context.ProductVariants.AnyAsync(v => v.VariantSku == variantDto.VariantSku && v.Product!.CompanyId == companyId);
               if (variantSkuExists) throw new InvalidOperationException($"Product Variant with SKU '{variantDto.VariantSku}' already exists.");
            }

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var product = new Product
                    {
                        TenantId = tenantId,
                        CompanyId = companyId,
                        ProductName = request.ProductName,
                        ProductSkuBase = request.ProductSkuBase,
                        CategoryId = request.CategoryId,
                        LocationId = request.LocationId,
                        SupplierId = request.SupplierId, // Save Global Supplier
                        Description = request.Description,
                        // Removed global variables from Product Table
                        IsActive = request.IsActive,
                        IsVatExcluded = request.IsVatExcluded,
                        Image1 = request.Image1,
                        CreatedBy = userId
                    };

                    _context.Products.Add(product);
                    await _context.SaveChangesAsync();

                     // Create Variants
                    if (request.Variants != null && request.Variants.Any())
                    {
                        foreach (var variantDto in request.Variants)
                        {
                            var variant = new ProductVariant
                            {
                                ProductId = product.Id,
                                VariantName = variantDto.VariantName,
                                VariantSku = variantDto.VariantSku,
                                Barcode = variantDto.Barcode,
                                // Removed global variables from ProductVariant Table
                                CreatedBy = userId
                            };
                            _context.ProductVariants.Add(variant);
                            await _context.SaveChangesAsync(); // Save to get ID

                            // Create Location-Specific Records (Inventory, Pricing, Cost)
                            if (request.LocationId.HasValue)
                            {
                                // Inventory
                                var inventory = new Inventory
                                {
                                    ProductId = product.Id,
                                    ProductVariantId = variant.Id,
                                    LocationId = request.LocationId.Value,
                                    InitialQuantity = variantDto.StockLevel ?? 0,
                                    Quantity = variantDto.StockLevel ?? 0,
                                    ReorderLevel = variantDto.MinStockLevel ?? 0,
                                    CreatedBy = userId,
                                    LastUpdated = DateTime.UtcNow
                                };
                                _context.Inventory.Add(inventory);

                                // Pricing
                                var pricing = new Pricing
                                {
                                    ProductId = product.Id,
                                    ProductVariantId = variant.Id,
                                    LocationId = request.LocationId.Value,
                                    CompanyId = companyId,
                                    TenantId = tenantId,
                                    Price = variantDto.Price ?? 0,
                                    CreatedBy = userId,
                                    IsActive = true
                                };
                                _context.Pricing.Add(pricing);

                                // Cost
                                var cost = new Cost
                                {
                                    ProductId = product.Id,
                                    ProductVariantId = variant.Id,
                                    LocationId = request.LocationId.Value,
                                    CompanyId = companyId,
                                    TenantId = tenantId,
                                    CostValue = variantDto.Cost ?? 0,
                                    SupplierId = request.SupplierId ?? throw new ArgumentException("Supplier is required for cost tracking"), // Enforce validation
                                    CreatedBy = userId,
                                    EffectiveDate = DateTime.UtcNow
                                };
                                _context.Costs.Add(cost);
                            }
                        }
                        await _context.SaveChangesAsync();
                        
                        // Product Totals removed
                    }
                    else
                    {
                         // Throw error? User said variant is required.
                         throw new InvalidOperationException("At least one product variant is required.");
                    }

                    // Optional: Create initial "Creation" audit log
                    var audit = new ProductAudit
                    {
                        ProductId = product.Id,
                        Action = "Create",
                        Reason = "Initial Creation",
                        ChangedBy = userId,
                        Changes = $"Created product with {request.Variants?.Count ?? 0} variants."
                    };
                    _context.ProductAudits.Add(audit);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return await GetProductByIdAsync(product.Id, tenantId);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<ProductDto> UpdateProductAsync(Guid productId, UpdateProductDto request, Guid tenantId, Guid userId)
        {
             var product = await _context.Products
                .Include(p => p.Variants) // Load Variants for update
                .FirstOrDefaultAsync(p => p.Id == productId && p.TenantId == tenantId);
             
             if (product == null) throw new KeyNotFoundException("Product not found.");

             // Check SKU uniqueness only if it changed
             if (product.ProductSkuBase != request.ProductSkuBase)
             {
                 var skuExists = await _context.Products.AnyAsync(p => p.ProductSkuBase == request.ProductSkuBase && p.CompanyId == product.CompanyId);
                 if (skuExists) throw new InvalidOperationException($"Product with SKU '{request.ProductSkuBase}' already exists.");
             }

             // CAPTURE CHANGES FOR AUDIT
             var audit = new ProductAudit
             {
                 ProductId = product.Id,
                 Action = "Update",
                 Reason = request.UpdateReason, // Explicit from DTO
                 ChangedBy = userId,
                 Changes = $"Updated fields. Previous Name: {product.ProductName}, Previous SKU: {product.ProductSkuBase}" // Simple summary
             };
             _context.ProductAudits.Add(audit);

             // Update Fields
             product.ProductName = request.ProductName;
             product.ProductSkuBase = request.ProductSkuBase;
             product.CategoryId = request.CategoryId;
             product.LocationId = request.LocationId;
             product.Description = request.Description;
             // Global Price/Cost/Stock updated from Variants aggregation below
             product.IsActive = request.IsActive;
             product.IsVatExcluded = request.IsVatExcluded;
             product.Image1 = request.Image1;
             product.UpdatedBy = userId;
             product.LastUpdated = DateTime.UtcNow;
             if (request.SupplierId.HasValue) product.SupplierId = request.SupplierId.Value; // Update Supplier

             // --- Variant Update Logic ---
             if (request.Variants != null)
             {
                 var incomingVariantIds = request.Variants.Where(v => v.Id.HasValue).Select(v => v.Id!.Value).ToList();

                 // 1. DELETE: Remove variants not in incoming list
                 var variantsToDelete = product.Variants.Where(v => !incomingVariantIds.Contains(v.Id)).ToList();
                 if (variantsToDelete.Any())
                 {
                     _context.ProductVariants.RemoveRange(variantsToDelete);
                     // Note: Cascading deletes for Inventory/Pricing/Cost should be handled by DB FKs or here if FKs are restrictive
                     // Assuming DB Cascade Delete or manual cleanup needed if strict
                 }

                 foreach (var variantDto in request.Variants)
                 {
                     if (variantDto.Id.HasValue)
                     {
                         // 2. UPDATE: Find existing
                         var existingVariant = product.Variants.FirstOrDefault(v => v.Id == variantDto.Id.Value);
                         if (existingVariant != null)
                         {
                             existingVariant.VariantName = variantDto.VariantName;
                             existingVariant.VariantSku = variantDto.VariantSku;
                             existingVariant.Barcode = variantDto.Barcode;
                             
                            // Removed global update of Price/Stock level
                             existingVariant.UpdatedBy = userId;
                             existingVariant.UpdatedAt = DateTime.UtcNow;
                             
                            // Update Related Logic (Inventory/Pricing/Cost) strictly for Current Location
                            // In a real app, this might be a separate adjustment endpoint, but user requested simplified update
                            if (request.LocationId.HasValue)
                            {
                                try 
                                {
                                    // Update Inventory
                                    var inv = await _context.Inventory.FirstOrDefaultAsync(i => i.ProductVariantId == existingVariant.Id && i.LocationId == request.LocationId.Value);
                                    if (inv == null) inv = _context.Inventory.Local.FirstOrDefault(i => i.ProductVariantId == existingVariant.Id && i.LocationId == request.LocationId.Value);

                                    if (inv != null) {
                                        inv.Quantity = variantDto.StockLevel ?? inv.Quantity; // Use distinct stock level if provided
                                        inv.ReorderLevel = variantDto.MinStockLevel ?? inv.ReorderLevel;
                                        // _context.Inventory.Update(inv); // Not needed if tracked, but harmless
                                    } else {
                                        // Create if missing
                                        _context.Inventory.Add(new Inventory { ProductId = product.Id, ProductVariantId = existingVariant.Id, LocationId = request.LocationId.Value, Quantity = variantDto.StockLevel ?? 0, InitialQuantity = variantDto.StockLevel ?? 0, CreatedBy = userId });
                                    }
                                    
                                    // Update Pricing
                                    var pricing = await _context.Pricing.FirstOrDefaultAsync(p => p.ProductVariantId == existingVariant.Id && p.LocationId == request.LocationId.Value);
                                    if (pricing == null) pricing = _context.Pricing.Local.FirstOrDefault(p => p.ProductVariantId == existingVariant.Id && p.LocationId == request.LocationId.Value);

                                    if (pricing != null) {
                                        if (variantDto.Price.HasValue) pricing.Price = variantDto.Price.Value;
                                        // _context.Pricing.Update(pricing);
                                    } else {
                                        _context.Pricing.Add(new Pricing { ProductId = product.Id, ProductVariantId = existingVariant.Id, LocationId = request.LocationId.Value, Price = variantDto.Price ?? 0, CompanyId = product.CompanyId, TenantId = tenantId, CreatedBy = userId });
                                    }
                                    
                                    // Update Cost
                                    var cost = await _context.Costs.FirstOrDefaultAsync(c => c.ProductVariantId == existingVariant.Id && c.LocationId == request.LocationId.Value);
                                    if (cost == null) cost = _context.Costs.Local.FirstOrDefault(c => c.ProductVariantId == existingVariant.Id && c.LocationId == request.LocationId.Value);

                                    if (cost != null) {
                                        if (variantDto.Cost.HasValue) cost.CostValue = variantDto.Cost.Value;
                                        if (request.SupplierId.HasValue) cost.SupplierId = request.SupplierId.Value; // Update Supplier if provided
                                    } else {
                                        _context.Costs.Add(new Cost { 
                                            ProductId = product.Id, 
                                            ProductVariantId = existingVariant.Id, 
                                            LocationId = request.LocationId.Value, 
                                            CostValue = variantDto.Cost ?? 0, 
                                            SupplierId = request.SupplierId ?? throw new ArgumentException("Supplier is required for cost tracking"), // Enforce validation
                                            CompanyId = product.CompanyId, 
                                            TenantId = tenantId, 
                                            CreatedBy = userId 
                                        });
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"[ERROR] Failed to update inventory/pricing/cost for variant {existingVariant.Id}: {ex}");
                                    throw; // Re-throw to fail the request but log it
                                }
                            }
                         }
                     }
                     else
                     {
                         // 3. ADD: Create new
                         var newVariant = new ProductVariant
                         {
                             ProductId = product.Id,
                             VariantName = variantDto.VariantName,
                             VariantSku = variantDto.VariantSku,
                             Barcode = variantDto.Barcode,
                             // No global properties
                             CreatedBy = userId
                         };
                         _context.ProductVariants.Add(newVariant);
                         await _context.SaveChangesAsync(); // Need ID

                         if (request.LocationId.HasValue)
                         {
                             _context.Inventory.Add(new Inventory { ProductId = product.Id, ProductVariantId = newVariant.Id, LocationId = request.LocationId.Value, Quantity = variantDto.StockLevel ?? 0, InitialQuantity = variantDto.StockLevel ?? 0, CreatedBy = userId });
                             _context.Pricing.Add(new Pricing { ProductId = product.Id, ProductVariantId = newVariant.Id, LocationId = request.LocationId.Value, Price = variantDto.Price ?? 0, CompanyId = product.CompanyId, TenantId = tenantId, CreatedBy = userId });
                             if(request.SupplierId.HasValue){
                                 _context.Costs.Add(new Cost { ProductId = product.Id, ProductVariantId = newVariant.Id, LocationId = request.LocationId.Value, CostValue = variantDto.Cost ?? 0, SupplierId = request.SupplierId.Value, CompanyId = product.CompanyId, TenantId = tenantId, CreatedBy = userId });
                             }
                         }
                     }
                 }
                 
                 // Totals not saved on product table anymore
                 await _context.SaveChangesAsync();
             }

             _context.Products.Update(product);
             await _context.SaveChangesAsync();

             return await GetProductByIdAsync(product.Id, tenantId);
        }
        public async Task<bool> DeleteProductAsync(Guid productId, Guid tenantId, Guid userId)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId && p.TenantId == tenantId);
            if (product == null) throw new KeyNotFoundException("Product not found.");

            product.IsActive = false;
            product.UpdatedBy = userId;
            product.UpdatedAt = DateTime.UtcNow;

            var audit = new ProductAudit
            {
                ProductId = product.Id,
                Action = "Delete",
                Reason = "Soft Delete",
                ChangedBy = userId,
                Changes = "Status changed to Inactive"
            };
            _context.ProductAudits.Add(audit);

            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> DeleteAllProductsAsync(Guid tenantId, Guid userId)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Fetch all products for this tenant
                    var products = await _context.Products
                        .Include(p => p.Variants)
                        .Where(p => p.TenantId == tenantId)
                        .ToListAsync();

                    if (!products.Any()) return 0;

                    var productIds  = products.Select(p => p.Id).ToList();
                    var variantIds  = products.SelectMany(p => p.Variants.Select(v => v.Id)).ToList();

                    // 1. Remove related Inventory records
                    var inventories = await _context.Inventory
                        .Where(i => productIds.Contains(i.ProductId))
                        .ToListAsync();
                    _context.Inventory.RemoveRange(inventories);

                    // 2. Remove related Pricing records
                    var pricings = await _context.Pricing
                        .Where(p => productIds.Contains(p.ProductId))
                        .ToListAsync();
                    _context.Pricing.RemoveRange(pricings);

                    // 3. Remove related Cost records
                    var costs = await _context.Costs
                        .Where(c => productIds.Contains(c.ProductId))
                        .ToListAsync();
                    _context.Costs.RemoveRange(costs);

                    // 4. Remove ProductAudit records
                    var audits = await _context.ProductAudits
                        .Where(a => productIds.Contains(a.ProductId))
                        .ToListAsync();
                    _context.ProductAudits.RemoveRange(audits);

                    // 5. Remove ProductVariants
                    var variants = await _context.ProductVariants
                        .Where(v => productIds.Contains(v.ProductId))
                        .ToListAsync();
                    _context.ProductVariants.RemoveRange(variants);

                    // 6. Remove Products
                    _context.Products.RemoveRange(products);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return products.Count;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }


        public async Task<int> BulkUploadProductsAsync(Stream fileStream, string fileName, Guid tenantId, Guid userId, Guid companyId)
        {
            var companyExists = await _context.Companies.AnyAsync(c => c.Id == companyId && c.TenantId == tenantId);
            if (!companyExists) throw new KeyNotFoundException("Company not found.");

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            int importedCount = 0;
            var newProducts = new List<Product>();
            var newVariants = new List<ProductVariant>();
            var newInventories = new List<Inventory>();
            var newPricings = new List<Pricing>();
            var newCosts = new List<Cost>();

            using (var reader = ExcelDataReader.ExcelReaderFactory.CreateReader(fileStream))
            {
                var result = reader.AsDataSet(new ExcelDataReader.ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = (_) => new ExcelDataReader.ExcelDataTableConfiguration()
                    {
                        UseHeaderRow = true
                    }
                });

                if (result.Tables.Count == 0) return 0;
                var table = result.Tables[0];

                foreach (System.Data.DataRow row in table.Rows)
                {
                    try
                    {
                        var productName = row["ProductName"]?.ToString();
                        if (string.IsNullOrWhiteSpace(productName)) continue;

                        var sku = row["Sku"]?.ToString();
                        if (string.IsNullOrWhiteSpace(sku)) sku = Guid.NewGuid().ToString().Substring(0, 8);

                        _ = Guid.TryParse(row["CategoryId"]?.ToString(), out Guid categoryId);
                        
                        Guid? locationId = null;
                        if (Guid.TryParse(row["LocationId"]?.ToString(), out Guid parsedLocId)) locationId = parsedLocId;

                        Guid? supplierId = null;
                        if (Guid.TryParse(row["SupplierId"]?.ToString(), out Guid parsedSuppId)) supplierId = parsedSuppId;

                        var description = row["Description"]?.ToString();
                        _ = decimal.TryParse(row["Price"]?.ToString(), out decimal price);
                        _ = decimal.TryParse(row["Cost"]?.ToString(), out decimal cost);
                        _ = int.TryParse(row["StockLevel"]?.ToString(), out int stockLevel);
                        _ = int.TryParse(row["MinStockLevel"]?.ToString(), out int minStockLevel);

                        // Avoid SKU collisions
                        if (await _context.Products.AnyAsync(p => p.ProductSkuBase == sku && p.CompanyId == companyId))
                        {
                            sku = sku + "-" + Guid.NewGuid().ToString().Substring(0, 4);
                        }

                        var product = new Product
                        {
                            Id = Guid.NewGuid(),
                            TenantId = tenantId,
                            CompanyId = companyId,
                            ProductName = productName,
                            ProductSkuBase = sku,
                            CategoryId = categoryId == Guid.Empty ? null : categoryId,
                            LocationId = locationId,
                            SupplierId = supplierId,
                            Description = description,
                            // Globals removed
                            IsActive = true,
                            IsVatExcluded = false,
                            CreatedBy = userId
                        };
                        newProducts.Add(product);
                        
                        var variant = new ProductVariant
                        {
                            Id = Guid.NewGuid(),
                            ProductId = product.Id,
                            VariantName = "Default",
                            VariantSku = sku + "-V1",
                            Barcode = sku,
                            // Globals removed
                            CreatedBy = userId
                        };
                        newVariants.Add(variant);

                        if (locationId.HasValue)
                        {
                            newInventories.Add(new Inventory { ProductId = product.Id, ProductVariantId = variant.Id, LocationId = locationId.Value, Quantity = stockLevel, InitialQuantity = stockLevel, ReorderLevel = minStockLevel, CreatedBy = userId, LastUpdated = DateTime.UtcNow });
                            newPricings.Add(new Pricing { ProductId = product.Id, ProductVariantId = variant.Id, LocationId = locationId.Value, Price = price, CompanyId = companyId, TenantId = tenantId, CreatedBy = userId, IsActive = true });
                            
                            // Only add costs if supplier exists
                            if (supplierId.HasValue && supplierId.Value != Guid.Empty)
                            {
                                newCosts.Add(new Cost { ProductId = product.Id, ProductVariantId = variant.Id, LocationId = locationId.Value, CostValue = cost, SupplierId = supplierId.Value, CompanyId = companyId, TenantId = tenantId, CreatedBy = userId, EffectiveDate = DateTime.UtcNow });
                            }
                        }

                        importedCount++;
                    }
                    catch
                    {
                        // Ignore individual row failures for batch processes
                        continue;
                    }
                }
            }

            if (importedCount > 0)
            {
                var strategy = _context.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        await _context.Products.AddRangeAsync(newProducts);
                        await _context.ProductVariants.AddRangeAsync(newVariants);
                        if (newInventories.Any()) await _context.Inventory.AddRangeAsync(newInventories);
                        if (newPricings.Any()) await _context.Pricing.AddRangeAsync(newPricings);
                        if (newCosts.Any()) await _context.Costs.AddRangeAsync(newCosts);

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });
            }

            return importedCount;
        }

        public async Task<IEnumerable<Guid>> GetProductLocationAssignmentsAsync(Guid productId, Guid tenantId)
        {
            // A product is "assigned" to a location if any of its variants have an Inventory record there
            var variants = await _context.ProductVariants
                .Where(v => v.ProductId == productId)
                .Select(v => v.Id)
                .ToListAsync();

            var locationIds = await _context.Inventory
                .Where(i => variants.Contains(i.ProductVariantId))
                .Select(i => i.LocationId)
                .Distinct()
                .ToListAsync();

            return locationIds;
        }

        public async Task UpdateProductLocationAssignmentsAsync(Guid productId, List<Guid> locationIds, Guid tenantId, Guid userId, Guid companyId)
        {
            var product = await _context.Products
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == productId && p.TenantId == tenantId);

            if (product == null) throw new KeyNotFoundException("Product not found.");

            var variantIds = product.Variants.Select(v => v.Id).ToList();

            // Get currently assigned locations
            var existingInventories = await _context.Inventory
                .Where(i => variantIds.Contains(i.ProductVariantId))
                .ToListAsync();

            var existingLocationIds = existingInventories.Select(i => i.LocationId).Distinct().ToList();

            // Locations to remove (in existing but not in new list)
            var locationsToRemove = existingLocationIds.Except(locationIds).ToList();
            if (locationsToRemove.Any())
            {
                var toDelete = existingInventories
                    .Where(i => locationsToRemove.Contains(i.LocationId) && i.Quantity == 0)
                    .ToList();
                _context.Inventory.RemoveRange(toDelete);
            }

            // Locations to add (in new list but not in existing)
            var locationsToAdd = locationIds.Except(existingLocationIds).ToList();
            foreach (var locationId in locationsToAdd)
            {
                // Verify location exists and belongs to this company
                var locationExists = await _context.Locations.AnyAsync(l => l.Id == locationId && l.CompanyId == companyId);
                if (!locationExists) continue;

                foreach (var variant in product.Variants)
                {
                    // Create Inventory record (quantity 0) at this location
                    var inventoryExists = await _context.Inventory
                        .AnyAsync(i => i.ProductVariantId == variant.Id && i.LocationId == locationId);
                    if (inventoryExists) continue;

                    _context.Inventory.Add(new Inventory
                    {
                        ProductId = productId,
                        ProductVariantId = variant.Id,
                        LocationId = locationId,
                        InitialQuantity = 0,
                        Quantity = 0,
                        ReorderLevel = 0,
                        CreatedBy = userId,
                        LastUpdated = DateTime.UtcNow
                    });

                    // Also create Pricing record if it doesn't exist
                    var pricingExists = await _context.Pricing
                        .AnyAsync(p => p.ProductVariantId == variant.Id && p.LocationId == locationId);
                    if (!pricingExists)
                    {
                        _context.Pricing.Add(new Pricing
                        {
                            ProductId = productId,
                            ProductVariantId = variant.Id,
                            LocationId = locationId,
                            Price = 0,
                            CompanyId = companyId,
                            TenantId = tenantId,
                            CreatedBy = userId,
                            IsActive = true
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
