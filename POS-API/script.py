import os
import re

file_path = "/Users/teksol-godwin/Documents/PERSONAL PROJECTS/POS Structure/POS-API/POS.Infrastructure/Services/ProductService.cs"

with open(file_path, "r") as f:
    content = f.read()

# 1. GetProductsAsync: Remove the foreach loop that updates entity directly and push DTO logic to the `.Select()`
pattern1 = r"""                foreach \(var product in products\)\s*\{\s*foreach \(var variant in product\.Variants\)\s*\{\s*if \(inventories\.TryGetValue\(variant\.Id, out var inv\)\)\s*\{\s*variant\.StockLevel = \(int\)inv\.Quantity;\s*variant\.MinStockLevel = \(int\)inv\.ReorderLevel;\s*\}\s*else\s*\{\s*variant\.StockLevel = 0;\s*\}\s*if \(pricings\.TryGetValue\(variant\.Id, out var price\)\)\s*\{\s*variant\.Price = price\.Price;\s*\}\s*\}\s*// Re-calculate Product Level aggregates for display\s*product\.StockLevel = product\.Variants\.Sum\(v => v\.StockLevel\);\s*if \(product\.Variants\.Any\(\)\)\s*\{\s*product\.Price = product\.Variants\.First\(\)\.Price; // Representative\s*product\.Cost = product\.Variants\.First\(\)\.Cost;\s*\}\s*\}"""

replacement1 = """                // Values are dynamically mapped during DTO projection"""

# 2. GetProductsAsync global stock update logic
pattern2 = r"""                foreach \(var product in products\)\s*\{\s*if \(globalInventory\.TryGetValue\(product\.Id, out var stock\)\)\s*\{\s*product\.StockLevel = \(int\)stock;\s*\}\s*else\s*\{\s*// Fallback: If no inventory records exist .*\s*// use the stock saved on the variants/product entity itself\.\s*product\.StockLevel = product\.Variants\.Sum\(v => v\.StockLevel\);\s*\}\s*\}"""

replacement2 = """                // Global stock values mapped dynamically during DTO projection"""

# 3. DTO mapping in GetProductsAsync
pattern3 = r"""            return products\.Select\(p => new ProductDto\s*\{\s*(.*?)\s*Price = p\.Price,\s*Cost = p\.Cost,\s*StockLevel = p\.StockLevel,\s*NewStock = p\.NewStock,\s*MinStockLevel = p\.MinStockLevel,\s*(.*?)\s*Variants = p\.Variants\.Select\(v => new ProductVariantDto\s*\{\s*(.*?)\s*Price = v\.Price,\s*Cost = v\.Cost,\s*StockLevel = v\.StockLevel,\s*MinStockLevel = v\.MinStockLevel,\s*(.*?)\s*\}\)\.ToList\(\)\s*\}\)\.ToList\(\);"""

replacement3 = """            return products.Select(p => {
                int pStock = 0;
                int pMinStock = 0;
                decimal pPrice = 0;
                decimal pCost = 0;

                if (!locationId.HasValue && globalInventory != null && globalInventory.TryGetValue(p.Id, out var gst)) {
                     pStock = (int)gst;
                }

                var variantsDto = p.Variants.Select(v => {
                    int vStock = 0;
                    int vMinStock = 0;
                    decimal vPrice = 0;
                    decimal vCost = 0;

                    if (locationId.HasValue) {
                        if (inventories != null && inventories.TryGetValue(v.Id, out var inv)) {
                            vStock = (int)inv.Quantity;
                            vMinStock = (int)inv.ReorderLevel;
                        }
                        if (pricings != null && pricings.TryGetValue(v.Id, out var prc)) vPrice = prc.Price;
                    }

                    return new ProductVariantDto {
                        \\3
                        Price = vPrice,
                        Cost = vCost,
                        StockLevel = vStock,
                        MinStockLevel = vMinStock,
                        \\4
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
                    \\1
                    Price = pPrice,
                    Cost = pCost,
                    StockLevel = pStock,
                    NewStock = 0,
                    MinStockLevel = pMinStock,
                    \\2
                    Variants = variantsDto
                };
            }).ToList();"""

# Remove hardcoded values from CreateProductAsync logic
pattern4 = r"""                        Price = request\.Price \?\? 0, // Assuming DTO has these, mapped from request\s*Cost = request\.Cost \?\? 0,\s*StockLevel = request\.StockLevel \?\? 0,\s*NewStock = request\.NewStock \?\? 0,\s*MinStockLevel = request\.MinStockLevel \?\? 0,"""
replacement4 = """                        // Removed global variables from Product Table"""

pattern5 = r"""                                Price = variantDto\.Price \?\? 0, // Stored on Variant as reference, but also in Pricing\s*Cost = variantDto\.Cost \?\? 0,   // Stored on Variant as reference, but also in Cost\s*StockLevel = variantDto\.StockLevel \?\? 0, // Reference\s*MinStockLevel = variantDto\.MinStockLevel \?\? 0,"""
replacement5 = """                                // Removed global variables from ProductVariant Table"""

pattern6 = r"""                                    InitialQuantity = variant\.StockLevel,\s*Quantity = variant\.StockLevel,\s*ReorderLevel = variant\.MinStockLevel,"""
replacement6 = """                                    InitialQuantity = variantDto.StockLevel ?? 0,
                                    Quantity = variantDto.StockLevel ?? 0,
                                    ReorderLevel = variantDto.MinStockLevel ?? 0,"""

pattern7 = r"""                                    Price = variant\.Price,"""
replacement7 = """                                    Price = variantDto.Price ?? 0,"""

pattern8 = r"""                                    CostValue = variant\.Cost,"""
replacement8 = """                                    CostValue = variantDto.Cost ?? 0,"""

pattern9 = r"""                        // Update Product Totals\s*product\.StockLevel = request\.Variants\.Sum\(v => v\.StockLevel \?\? 0\);\s*// Optional: Set Product Price/Cost to first variant/min/max for display consistency\s*if \(request\.Variants\.Any\(\)\) \s*\{\s*product\.Price = request\.Variants\.First\(\)\.Price \?\? 0;\s*product\.Cost = request\.Variants\.First\(\)\.Cost \?\? 0;\s*\}\s*_context\.Products\.Update\(product\);\s*await _context\.SaveChangesAsync\(\);"""
replacement9 = """                        // Product Totals removed"""

# Re-run regex replacements for GetProductByIdAsync
pattern10 = r"""                foreach \(var variant in product\.Variants\)\s*\{\s*if \(inventories\.TryGetValue\(variant\.Id, out var inv\)\)\s*\{\s*variant\.StockLevel = \(int\)inv\.Quantity;\s*variant\.MinStockLevel = \(int\)inv\.ReorderLevel;\s*\} \s*else variant\.StockLevel = 0;\s*if \(pricings\.TryGetValue\(variant\.Id, out var price\)\) variant\.Price = price\.Price;\s*// if \(costs\.TryGetValue\(variant\.Id, out var cost\)\) variant\.Cost = cost\.CostValue; // Costs no longer fetched this way\s*\}\s*// Aggregates\s*product\.StockLevel = product\.Variants\.Sum\(v => v\.StockLevel\);\s*if \(product\.Variants\.Any\(\)\)\s*\{\s*product\.Price = product\.Variants\.First\(\)\.Price;\s*product\.Cost = product\.Variants\.First\(\)\.Cost;\s*\}"""
replacement10 = """                // Values dynamic"""

pattern11 = r"""                 // Global View: Calculate Stock from Inventory Table \(Source of Truth\)\s*var totalStock = await _context\.Inventory\s*\.Where\(i => i\.ProductId == product\.Id\)\s*\.SumAsync\(i => i\.Quantity\);\s*product\.StockLevel = \(int\)totalStock;"""
replacement11 = """                 // Values dynamic
                 var totalStock = await _context.Inventory
                     .Where(i => i.ProductId == product.Id)
                     .SumAsync(i => i.Quantity);"""


pattern12 = r"""            return new ProductDto\s*\{\s*(.*?)\s*Price = product\.Price,\s*Cost = product\.Cost,\s*StockLevel = product\.StockLevel,\s*NewStock = product\.NewStock,\s*MinStockLevel = product\.MinStockLevel,\s*(.*?)\s*Variants = product\.Variants\.Select\(v => new ProductVariantDto\s*\{\s*(.*?)\s*Price = v\.Price,\s*Cost = v\.Cost,\s*StockLevel = v\.StockLevel,\s*MinStockLevel = v\.MinStockLevel,\s*(.*?)\s*\}\)\.ToList\(\)\s*\};"""

replacement12 = """            int pStock = 0;
            int pMinStock = 0;
            decimal pPrice = 0;
            decimal pCost = 0;

            if (!locationId.HasValue) {
                pStock = (int)totalStock;
            }

            var variantsDto = product.Variants.Select(v => {
                int vStock = 0;
                int vMinStock = 0;
                decimal vPrice = 0;
                decimal vCost = 0;

                if (locationId.HasValue) {
                    if (inventories != null && inventories.TryGetValue(v.Id, out var inv)) {
                        vStock = (int)inv.Quantity;
                        vMinStock = (int)inv.ReorderLevel;
                    }
                    if (pricings != null && pricings.TryGetValue(v.Id, out var prc)) vPrice = prc.Price;
                }

                return new ProductVariantDto {
                    \\3
                    Price = vPrice,
                    Cost = vCost,
                    StockLevel = vStock,
                    MinStockLevel = vMinStock,
                    \\4
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
                \\1
                Price = pPrice,
                Cost = pCost,
                StockLevel = pStock,
                NewStock = 0,
                MinStockLevel = pMinStock,
                \\2
                Variants = variantsDto
            };"""

# Update logic
pattern13 = r"""                             // Only update Master/Global values if this is a Global Update \(No Location Selected\)\s*if \(!request\.LocationId\.HasValue\)\s*\{\s*existingVariant\.Price = variantDto\.Price \?\? existingVariant\.Price;\s*existingVariant\.Cost = variantDto\.Cost \?\? existingVariant\.Cost;\s*existingVariant\.StockLevel = variantDto\.StockLevel \?\? existingVariant\.StockLevel;\s*existingVariant\.MinStockLevel = variantDto\.MinStockLevel \?\? existingVariant\.MinStockLevel;\s*\}"""
replacement13 = """                             // Removed global update of Price/Stock level"""

pattern14 = r"""                                        pricing\.Price = variantDto\.Price \?\? pricing\.Price; // FIX: Use incoming price"""
replacement14 = """                                        if (variantDto.Price.HasValue) pricing.Price = variantDto.Price.Value;"""
pattern15 = r"""                                        _context\.Pricing\.Add\(new Pricing \{ ProductId = product\.Id, ProductVariantId = existingVariant\.Id, LocationId = request\.LocationId\.Value, Price = variantDto\.Price \?\? existingVariant\.Price, CompanyId = product\.CompanyId, TenantId = tenantId, CreatedBy = userId \}\);"""
replacement15 = """                                        _context.Pricing.Add(new Pricing { ProductId = product.Id, ProductVariantId = existingVariant.Id, LocationId = request.LocationId.Value, Price = variantDto.Price ?? 0, CompanyId = product.CompanyId, TenantId = tenantId, CreatedBy = userId });"""

pattern16 = r"""                                        cost\.CostValue = existingVariant\.Cost;"""
replacement16 = """                                        if (variantDto.Cost.HasValue) cost.CostValue = variantDto.Cost.Value;"""
pattern17 = r"""                                             CostValue = existingVariant\.Cost,"""
replacement17 = """                                             CostValue = variantDto.Cost ?? 0,"""

pattern18 = r"""                             Price = variantDto\.Price \?\? 0,\s*Cost = variantDto\.Cost \?\? 0,\s*StockLevel = variantDto\.StockLevel \?\? 0,\s*MinStockLevel = variantDto\.MinStockLevel \?\? 0,"""
replacement18 = """                             // No global properties"""

pattern19 = r"""                             _context\.Inventory\.Add\(new Inventory \{ ProductId = product\.Id, ProductVariantId = newVariant\.Id, LocationId = request\.LocationId\.Value, Quantity = newVariant\.StockLevel, InitialQuantity = newVariant\.StockLevel, CreatedBy = userId \}\);\s*_context\.Pricing\.Add\(new Pricing \{ ProductId = product\.Id, ProductVariantId = newVariant\.Id, LocationId = request\.LocationId\.Value, Price = newVariant\.Price, CompanyId = product\.CompanyId, TenantId = tenantId, CreatedBy = userId \}\);\s*_context\.Costs\.Add\(new Cost \{ ProductId = product\.Id, ProductVariantId = newVariant\.Id, LocationId = request\.LocationId\.Value, CostValue = newVariant\.Cost, CompanyId = product\.CompanyId, TenantId = tenantId, CreatedBy = userId \}\);"""
replacement19 = """                             _context.Inventory.Add(new Inventory { ProductId = product.Id, ProductVariantId = newVariant.Id, LocationId = request.LocationId.Value, Quantity = variantDto.StockLevel ?? 0, InitialQuantity = variantDto.StockLevel ?? 0, CreatedBy = userId });
                             _context.Pricing.Add(new Pricing { ProductId = product.Id, ProductVariantId = newVariant.Id, LocationId = request.LocationId.Value, Price = variantDto.Price ?? 0, CompanyId = product.CompanyId, TenantId = tenantId, CreatedBy = userId });
                             if(request.SupplierId.HasValue){
                                 _context.Costs.Add(new Cost { ProductId = product.Id, ProductVariantId = newVariant.Id, LocationId = request.LocationId.Value, CostValue = variantDto.Cost ?? 0, SupplierId = request.SupplierId.Value, CompanyId = product.CompanyId, TenantId = tenantId, CreatedBy = userId });
                             }"""
pattern20 = r"""                 // Recalculate Totals\s*await _context\.SaveChangesAsync\(\); // Ensure all variants saved\s*product\.StockLevel = product\.Variants\.Sum\(v => v\.StockLevel\);\s*if \(product\.Variants\.Any\(\)\) \{\s*product\.Price = product\.Variants\.First\(\)\.Price;\s*product\.Cost = product\.Variants\.First\(\)\.Cost;\s*\}"""
replacement20 = """                 // Totals not saved on product table anymore
                 await _context.SaveChangesAsync();"""

pattern21 = r"""                        Price = price,\s*Cost = cost,\s*StockLevel = stockLevel,\s*MinStockLevel = minStockLevel,"""
replacement21 = """                        // Globals removed"""


content = re.sub(pattern1, replacement1, content, flags=re.DOTALL)
content = re.sub(pattern2, replacement2, content, flags=re.DOTALL)
content = re.sub(pattern3, replacement3, content, flags=re.DOTALL)

content = re.sub(pattern4, replacement4, content, flags=re.DOTALL)
content = re.sub(pattern5, replacement5, content, flags=re.DOTALL)
content = re.sub(pattern6, replacement6, content, flags=re.DOTALL)
content = re.sub(pattern7, replacement7, content, flags=re.DOTALL)
content = re.sub(pattern8, replacement8, content, flags=re.DOTALL)
content = re.sub(pattern9, replacement9, content, flags=re.DOTALL)

content = re.sub(pattern10, replacement10, content, flags=re.DOTALL)
content = re.sub(pattern11, replacement11, content, flags=re.DOTALL)
content = re.sub(pattern12, replacement12, content, flags=re.DOTALL)

content = re.sub(pattern13, replacement13, content, flags=re.DOTALL)
content = re.sub(pattern14, replacement14, content, flags=re.DOTALL)
content = re.sub(pattern15, replacement15, content, flags=re.DOTALL)
content = re.sub(pattern16, replacement16, content, flags=re.DOTALL)
content = re.sub(pattern17, replacement17, content, flags=re.DOTALL)
content = re.sub(pattern18, replacement18, content, flags=re.DOTALL)
content = re.sub(pattern19, replacement19, content, flags=re.DOTALL)
content = re.sub(pattern20, replacement20, content, flags=re.DOTALL)
content = re.sub(pattern21, replacement21, content, flags=re.DOTALL)

with open(file_path, "w") as f:
    f.write(content)
print("Changes applied!")
