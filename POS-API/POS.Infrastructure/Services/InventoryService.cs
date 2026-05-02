using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs.Inventory;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Infrastructure.Persistence;

namespace POS.Infrastructure.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAccountingService _accountingService;

        public InventoryService(ApplicationDbContext context, IAccountingService accountingService)
        {
            _context = context;
            _accountingService = accountingService;
        }

        public async Task<IEnumerable<InventoryDto>> GetAllInventoriesAsync(Guid tenantId)
        {
            var inventories = await _context.Inventory
                .Include(i => i.Product)
                .Include(i => i.ProductVariant)
                .Include(i => i.Location)
                .Where(i => i.Product!.TenantId == tenantId)
                .ToListAsync();

            return inventories.Select(MapToDto).ToList();
        }

        public async Task<InventoryDto> GetInventoryAsync(Guid productVariantId, Guid locationId, Guid tenantId)
        {
            var inventory = await _context.Inventory
                .Include(i => i.Product)
                .Include(i => i.ProductVariant)
                .Include(i => i.Location)
                .FirstOrDefaultAsync(i => i.ProductVariantId == productVariantId && i.LocationId == locationId && i.Product!.TenantId == tenantId);

            if (inventory == null) return null;

            return MapToDto(inventory);
        }

        public async Task<InventoryDto> GetInventoryByIdAsync(Guid inventoryId, Guid tenantId)
        {
            var inventory = await _context.Inventory
                 .Include(i => i.Product)
                 .Include(i => i.ProductVariant)
                 .Include(i => i.Location)
                 .FirstOrDefaultAsync(i => i.Id == inventoryId && i.Product!.TenantId == tenantId);

            if (inventory == null) throw new KeyNotFoundException("Inventory record not found.");

            return MapToDto(inventory);
        }

        public async Task<InventoryDto> CreateInventoryAsync(CreateInventoryDto request, Guid tenantId, Guid userId, Guid companyId)
        {
            // Check if already exists
            var existingInventory = await _context.Inventory
                .FirstOrDefaultAsync(i => 
                    i.ProductVariantId == request.ProductVariantId && 
                    i.LocationId == request.LocationId && 
                    i.Product!.TenantId == tenantId);

            if (existingInventory != null)
            {
                throw new InvalidOperationException($"Inventory for this Variant and Location already exists (ID: {existingInventory.Id}). Use Adjust if you need to update quantity.");
            }

            // Verify ProductVariant and Location
             var variant = await _context.ProductVariants
                 .Include(v => v.Product)
                 .FirstOrDefaultAsync(v => v.Id == request.ProductVariantId);

             if (variant == null || variant.Product!.TenantId != tenantId) 
                throw new KeyNotFoundException("Product Variant not found.");
            
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var inventory = new Inventory
                    {
                        ProductId = variant.ProductId,
                        ProductVariantId = request.ProductVariantId,
                        LocationId = request.LocationId,
                        InitialQuantity = request.InitialQuantity,
                        Quantity = request.Quantity,
                        TotalQuantitySold = 0,
                        TotalQuantityReturned = 0,
                        CreatedBy = userId,
                        LastUpdated = DateTime.UtcNow,
                        UpdatedBy = userId
                    };
                    _context.Inventory.Add(inventory);
                    await _context.SaveChangesAsync();

                    if (request.Quantity > 0)
                    {
                        var invTransaction = new InventoryTransaction
                        {
                            InventoryId = inventory.Id,
                            QuantityChanged = request.Quantity,
                            OldQuantity = 0,
                            NewQuantity = request.Quantity,
                            TransactionType = "InitialStock",
                            Reason = "Initial Inventory Creation",
                            CreatedBy = userId,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.InventoryTransactions.Add(invTransaction);
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();
                    
                    return await GetInventoryByIdAsync(inventory.Id, tenantId);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<InventoryDto> AdjustInventoryAsync(AdjustInventoryDto request, Guid tenantId, Guid userId, Guid companyId)
        {
            if (_context.Database.CurrentTransaction != null)
            {
                return await AdjustInventoryCoreAsync(request, tenantId, userId, companyId);
            }

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var result = await AdjustInventoryCoreAsync(request, tenantId, userId, companyId);
                    await transaction.CommitAsync();
                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        private async Task<InventoryDto> AdjustInventoryCoreAsync(AdjustInventoryDto request, Guid tenantId, Guid userId, Guid companyId)
        {
            // 1. Fetch Inventory or Create if not exists
            var inventory = await _context.Inventory
                .Include(i => i.Product)
                .Include(i => i.ProductVariant)
                .Include(i => i.Location)
                .FirstOrDefaultAsync(i => 
                    i.ProductVariantId == request.ProductVariantId && 
                    i.LocationId == request.LocationId && 
                    i.Product!.TenantId == tenantId);

            if (inventory == null)
            {
                    var variant = await _context.ProductVariants
                        .Include(v => v.Product)
                        .FirstOrDefaultAsync(v => v.Id == request.ProductVariantId);

                    if (variant == null || variant.Product!.TenantId != tenantId) 
                    throw new KeyNotFoundException("Product Variant not found.");

                inventory = new Inventory
                {
                    ProductId = variant.ProductId,
                    ProductVariantId = request.ProductVariantId,
                    LocationId = request.LocationId,
                    InitialQuantity = 0,
                    Quantity = 0,
                    TotalQuantitySold = 0,
                    TotalQuantityReturned = 0,
                    CreatedBy = userId,
                };
                _context.Inventory.Add(inventory);
                await _context.SaveChangesAsync(); 
            }

            // 2. Prepare Transaction
            var oldQuantity = inventory.Quantity;
            var newQuantity = oldQuantity + request.AdjustmentQuantity;
            
            // Allow negative stock? For now, enforcing non-negative.
            if (newQuantity < 0)
            {
                throw new InvalidOperationException($"Insufficient stock. Current: {oldQuantity}, Adjustment: {request.AdjustmentQuantity}");
            }

            var invTransaction = new InventoryTransaction
            {
                InventoryId = inventory.Id,
                QuantityChanged = request.AdjustmentQuantity,
                OldQuantity = oldQuantity,
                NewQuantity = newQuantity,
                TransactionType = request.TransactionType,
                Reason = request.Reason,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.InventoryTransactions.Add(invTransaction);

            // 3. Update Inventory Snapshot
            inventory.Quantity = newQuantity;
            inventory.LastUpdated = DateTime.UtcNow;
            inventory.UpdatedBy = userId;
            
            if (request.TransactionType == "StockOut" || request.TransactionType == "Sale")
            {
                inventory.TotalQuantitySold += Math.Abs(request.AdjustmentQuantity);
            }
            if (request.TransactionType == "Return")
            {
                inventory.TotalQuantityReturned += Math.Abs(request.AdjustmentQuantity);
            }

            _context.Inventory.Update(inventory);

            // (Removed updating global Master Stock Level on Variant/Product tables)

            await _context.SaveChangesAsync();

            // Accounting - Should be part of same transaction? Yes.
            try
            {
                await _accountingService.PostInventoryAdjustmentAsync(inventory.Id, request.AdjustmentQuantity, request.Reason, userId);
            }
            catch { /* Log error */ }

            return MapToDto(inventory);
        }

        private static InventoryDto MapToDto(Inventory i)
        {
            return new InventoryDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product?.ProductName,
                ProductVariantId = i.ProductVariantId,
                VariantName = i.ProductVariant?.VariantName,
                LocationId = i.LocationId,
                LocationName = i.Location?.LocationName,
                Quantity = i.Quantity,
                LastUpdated = i.LastUpdated
            };
        }
    }
}
