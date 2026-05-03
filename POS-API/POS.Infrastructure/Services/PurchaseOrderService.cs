using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs.PurchaseOrder;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Infrastructure.Persistence;

namespace POS.Infrastructure.Services
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IInventoryService _inventoryService;
        private readonly IGoodsReceiptService _goodsReceiptService;
        private readonly ISupplierInvoiceService _supplierInvoiceService;

        public PurchaseOrderService(
            ApplicationDbContext context, 
            IInventoryService inventoryService,
            IGoodsReceiptService goodsReceiptService,
            ISupplierInvoiceService supplierInvoiceService)
        {
            _context = context;
            _inventoryService = inventoryService;
            _goodsReceiptService = goodsReceiptService;
            _supplierInvoiceService = supplierInvoiceService;
        }

        public async Task<IEnumerable<PurchaseOrderDto>> GetPurchaseOrdersAsync(Guid tenantId, Guid companyId)
        {
            var orders = await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.Location)
                .Include(po => po.Details)
                    .ThenInclude(d => d.Product)
                .Include(po => po.Details)
                    .ThenInclude(d => d.ProductVariant)
                .Where(po => po.TenantId == tenantId && po.CompanyId == companyId)
                .OrderByDescending(po => po.OrderDate)
                .ToListAsync();

            return orders.Select(po => MapToDto(po)).ToList();
        }

        public async Task<PurchaseOrderDto> GetPurchaseOrderByIdAsync(Guid id, Guid tenantId)
        {
            var order = await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.Location)
                .Include(po => po.Details)
                    .ThenInclude(d => d.Product)
                .Include(po => po.Details)
                    .ThenInclude(d => d.ProductVariant)
                .FirstOrDefaultAsync(po => po.Id == id && po.TenantId == tenantId);

            if (order == null) throw new KeyNotFoundException("Purchase Order not found.");

            return MapToDto(order);
        }

        public async Task<PurchaseOrderDto> CreatePurchaseOrderAsync(CreatePurchaseOrderDto dto, Guid tenantId, Guid userId, Guid companyId)
        {
            // Validate Logic using Transaction
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 1. Validate Supplier & Location
                    if (dto.SupplierId.HasValue)
                    {
                        var supplierExists = await _context.Suppliers.AnyAsync(s => s.Id == dto.SupplierId.Value && s.TenantId == tenantId);
                        if (!supplierExists) throw new KeyNotFoundException("Supplier not found.");
                    }

                    var locationExists = await _context.Locations.AnyAsync(l => l.Id == dto.LocationId && l.TenantId == tenantId);
                    if (!locationExists) throw new KeyNotFoundException("Location not found.");

                    // 2. Create Header
                    var purchaseOrder = new PurchaseOrder
                    {
                        TenantId = tenantId,
                        CompanyId = companyId,
                        LocationId = dto.LocationId,
                        SupplierId = dto.SupplierId,
                        OrderDate = DateTime.UtcNow,
                        ExpectedDeliveryDate = dto.ExpectedDeliveryDate,
                        Status = "Pending",
                        Notes = dto.Notes,
                        CreatedBy = userId,
                        Details = new List<PurchaseOrderDetail>()
                    };

                    decimal totalAmount = 0;

                    // 3. Create Details
                    foreach (var itemDto in dto.Details)
                    {
                        var variant = await _context.ProductVariants
                            .Include(pv => pv.Product)
                            .FirstOrDefaultAsync(pv => pv.Id == itemDto.ProductVariantId && pv.Product!.TenantId == tenantId);
                        
                        if (variant == null) throw new KeyNotFoundException($"Product Variant {itemDto.ProductVariantId} not found.");

                        var lineTotal = itemDto.Quantity * itemDto.UnitCost;
                        totalAmount += lineTotal;

                        purchaseOrder.Details.Add(new PurchaseOrderDetail
                        {
                            // TenantId not in Detail entity
                            ProductId = variant.ProductId,
                            ProductVariantId = variant.Id,
                            Quantity = itemDto.Quantity,
                            UnitCost = itemDto.UnitCost,
                            LineTotal = lineTotal,
                            QuantityReceived = 0
                        });
                    }

                    purchaseOrder.TotalAmount = totalAmount;

                    _context.PurchaseOrders.Add(purchaseOrder);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Reload to get names included
                    return await GetPurchaseOrderByIdAsync(purchaseOrder.Id, tenantId);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<PurchaseOrderDto> UpdatePurchaseOrderAsync(Guid id, UpdatePurchaseOrderDto dto, Guid tenantId, Guid userId)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var order = await _context.PurchaseOrders
                        .Include(po => po.Details)
                        .FirstOrDefaultAsync(po => po.Id == id && po.TenantId == tenantId);

                    if (order == null) throw new KeyNotFoundException("Purchase Order not found.");

                    if (dto.ExpectedDeliveryDate.HasValue) order.ExpectedDeliveryDate = dto.ExpectedDeliveryDate;
                    if (!string.IsNullOrEmpty(dto.Status)) order.Status = dto.Status;
                    if (dto.Notes != null) order.Notes = dto.Notes;
                    order.LastUpdated = DateTime.UtcNow;
                    order.UpdatedBy = userId;

                    // Sync Details
                    // Map ID -> DTO
                    var dtoDetailsDict = dto.Details
                        .Where(d => d.Id.HasValue)
                        .ToDictionary(d => d.Id!.Value);

                    // Identify to remove
                    var toRemove = order.Details
                        .Where(existing => !dtoDetailsDict.ContainsKey(existing.Id))
                        .ToList();

                    foreach (var rem in toRemove)
                    {
                        _context.PurchaseOrderDetails.Remove(rem);
                        order.Details.Remove(rem);
                    }

                    // Update Existing & Add New
                    foreach (var itemDto in dto.Details)
                    {
                        if (itemDto.Id.HasValue && dtoDetailsDict.TryGetValue(itemDto.Id.Value, out _))
                        {
                            // Update
                            var existing = order.Details.FirstOrDefault(d => d.Id == itemDto.Id.Value);
                            if (existing != null)
                            {
                                existing.Quantity = itemDto.Quantity;
                                existing.UnitCost = itemDto.UnitCost;
                                existing.LineTotal = itemDto.Quantity * itemDto.UnitCost;
                                
                                if (existing.ProductVariantId != itemDto.ProductVariantId)
                                {
                                     var variant = await _context.ProductVariants.FirstOrDefaultAsync(v => v.Id == itemDto.ProductVariantId);
                                     if(variant != null) {
                                         existing.ProductVariantId = variant.Id;
                                         existing.ProductId = variant.ProductId;
                                     }
                                }
                            }
                        }
                        else
                        {
                            // Add New
                            var variant = await _context.ProductVariants
                                .Include(pv => pv.Product)
                                .FirstOrDefaultAsync(pv => pv.Id == itemDto.ProductVariantId && pv.Product!.TenantId == tenantId);
                             if (variant == null) throw new KeyNotFoundException($"Product Variant {itemDto.ProductVariantId} not found.");

                             var detail = new PurchaseOrderDetail
                             {
                                 PurchaseOrderId = order.Id, // Ensure link
                                 ProductId = variant.ProductId,
                                 ProductVariantId = variant.Id,
                                 Quantity = itemDto.Quantity,
                                 UnitCost = itemDto.UnitCost,
                                 LineTotal = itemDto.Quantity * itemDto.UnitCost,
                                 QuantityReceived = 0
                             };
                             _context.PurchaseOrderDetails.Add(detail);
                             order.Details.Add(detail);
                        }
                    }

                    // Recalculate Total
                    order.TotalAmount = order.Details.Sum(d => d.LineTotal);

                    _context.PurchaseOrders.Update(order);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return await GetPurchaseOrderByIdAsync(id, tenantId);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<PurchaseOrderDto> ReceivePurchaseOrderAsync(Guid id, Guid tenantId, Guid userId)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var order = await _context.PurchaseOrders
                        .Include(po => po.Details)
                        .FirstOrDefaultAsync(po => po.Id == id && po.TenantId == tenantId);

                    if (order == null) throw new KeyNotFoundException("Purchase Order not found.");
                    if (order.Status == "Received" || order.Status == "Cancelled") 
                        throw new InvalidOperationException($"Cannot receive order with status {order.Status}");

                    var receipt = new GoodsReceipt
                    {
                        TenantId = tenantId,
                        CompanyId = order.CompanyId,
                        LocationId = order.LocationId,
                        SupplierId = order.SupplierId,
                        PurchaseOrderId = order.Id,
                        ReceiptDate = DateTime.UtcNow,
                        Status = "Approved", // Implicitly approved since received
                        CreatedBy = userId,
                        ApprovedBy = userId,
                        ApprovedAt = DateTime.UtcNow,
                        Details = new List<GoodsReceiptDetail>()
                    };

                    decimal receiptTotal = 0;

                    // Iterate Details and Update Inventory
                    foreach (var detail in order.Details)
                    {
                        var qtyToReceive = detail.Quantity - detail.QuantityReceived;
                        if (qtyToReceive <= 0) continue;

                        var poNumber = $"PO-{order.OrderDate:yyyyMMdd}-{order.Id.ToString().Substring(0, 4).ToUpper()}";
                        await _inventoryService.AdjustInventoryAsync(new POS.Application.DTOs.Inventory.AdjustInventoryDto
                        {
                            ProductVariantId = detail.ProductVariantId,
                            LocationId = order.LocationId,
                            AdjustmentQuantity = qtyToReceive,
                            TransactionType = "GoodsReceipt",
                            Reason = $"Received from {poNumber}" 
                        }, tenantId, userId, order.CompanyId);

                        detail.QuantityReceived += qtyToReceive;

                        var lineTotal = qtyToReceive * detail.UnitCost;
                        receiptTotal += lineTotal;

                        receipt.Details.Add(new GoodsReceiptDetail
                        {
                            ProductId = detail.ProductId,
                            ProductVariantId = detail.ProductVariantId,
                            QuantityReceived = qtyToReceive,
                            UnitCost = detail.UnitCost,
                            LineTotal = lineTotal
                        });
                    }

                    receipt.TotalReceivedAmount = receiptTotal;
                    _context.GoodsReceipts.Add(receipt);

                    order.Status = "Received";
                    order.LastUpdated = DateTime.UtcNow;
                    order.UpdatedBy = userId;

                    _context.PurchaseOrders.Update(order);
                    await _context.SaveChangesAsync();
                    
                    if (receipt.SupplierId.HasValue)
                    {
                        var invoiceDto = new POS.Application.DTOs.SupplierInvoice.CreateSupplierInvoiceDto
                        {
                            SupplierId = receipt.SupplierId.Value,
                            GoodsReceiptId = receipt.Id,
                            InvoiceNumber = $"INV-{receipt.Id.ToString().Substring(0, 8).ToUpper()}",
                            InvoiceDate = receipt.ReceiptDate,
                            DueDate = receipt.ReceiptDate.AddDays(30),
                            TotalAmount = receipt.TotalReceivedAmount
                        };
                        // Note: Since we are inside PO transaction, we can call this assuming it doesn't nest explicitly
                        // Wait, creating invoice needs to happen after receipt is saved to get its DB-generated stuff if any
                        await _supplierInvoiceService.CreateInvoiceAsync(invoiceDto, tenantId, userId, order.CompanyId);
                    }

                    await transaction.CommitAsync();

                    return await GetPurchaseOrderByIdAsync(id, tenantId);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<PurchaseOrderDto> InstantPurchaseAsync(CreatePurchaseOrderDto dto, Guid tenantId, Guid userId, Guid companyId)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 1. Create Purchase Order (This internally saves changes, but transaction is not committed)
                    // We must bypass the standard Service layer's transaction if we want atomicity across all operations, 
                    // or rely on nested transactions/savepoints if supported. For simplicity, we write the entity logic directly 
                    // to keep it within *this* specific transaction scope and avoid committing early.

                    if (dto.SupplierId.HasValue)
                    {
                        var supplierExists = await _context.Suppliers.AnyAsync(s => s.Id == dto.SupplierId.Value && s.TenantId == tenantId);
                        if (!supplierExists) throw new KeyNotFoundException("Supplier not found.");
                    }

                    var purchaseOrder = new PurchaseOrder
                    {
                        TenantId = tenantId,
                        CompanyId = companyId,
                        LocationId = dto.LocationId,
                        SupplierId = dto.SupplierId,
                        OrderDate = DateTime.UtcNow,
                        ExpectedDeliveryDate = dto.ExpectedDeliveryDate ?? DateTime.UtcNow,
                        Status = "Received", // Automatically Mark Received
                        Notes = dto.Notes ?? "Instant Purchase Flow",
                        CreatedBy = userId,
                        Details = new List<PurchaseOrderDetail>()
                    };

                    decimal totalAmount = 0;
                    foreach (var itemDto in dto.Details)
                    {
                        var variant = await _context.ProductVariants
                            .Include(pv => pv.Product)
                            .FirstOrDefaultAsync(pv => pv.Id == itemDto.ProductVariantId && pv.Product!.TenantId == tenantId);
                        
                        if (variant == null) throw new KeyNotFoundException($"Product Variant {itemDto.ProductVariantId} not found.");

                        var lineTotal = itemDto.Quantity * itemDto.UnitCost;
                        totalAmount += lineTotal;

                        purchaseOrder.Details.Add(new PurchaseOrderDetail
                        {
                            ProductId = variant.ProductId,
                            ProductVariantId = variant.Id,
                            Quantity = itemDto.Quantity,
                            UnitCost = itemDto.UnitCost,
                            LineTotal = lineTotal,
                            QuantityReceived = itemDto.Quantity // Instant receive
                        });

                        // Instantly Adjust Inventory directly here to maintain atomic Tx
                        var poNumber = $"PO-{purchaseOrder.OrderDate:yyyyMMdd}-INSTANT";
                        await _inventoryService.AdjustInventoryAsync(new POS.Application.DTOs.Inventory.AdjustInventoryDto
                        {
                            ProductVariantId = variant.Id,
                            LocationId = purchaseOrder.LocationId,
                            AdjustmentQuantity = itemDto.Quantity,
                            TransactionType = "PurchaseOrder",
                            Reason = $"Instant Received {poNumber}" 
                        }, tenantId, userId, companyId);
                    }

                    purchaseOrder.TotalAmount = totalAmount;
                    _context.PurchaseOrders.Add(purchaseOrder);
                    await _context.SaveChangesAsync(); // Generates ID for the PO

                    // Supplier Invoice and Goods Receipt are deliberately skipped for Instant Purchases.
                    // The stock has already been directly adjusted above.

                    await transaction.CommitAsync();

                    return await GetPurchaseOrderByIdAsync(purchaseOrder.Id, tenantId);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        private static PurchaseOrderDto MapToDto(PurchaseOrder po)
        {
            var dto = new PurchaseOrderDto
            {
                Id = po.Id,
                CompanyId = po.CompanyId,
                LocationId = po.LocationId,
                LocationName = po.Location?.LocationName ?? "Unknown",
                PoNumber = $"PO-{po.OrderDate:yyyyMMdd}-{po.Id.ToString().Substring(0, 4).ToUpper()}",
                SupplierId = po.SupplierId,
                SupplierName = po.Supplier?.SupplierName,
                OrderDate = po.OrderDate,
                ExpectedDeliveryDate = po.ExpectedDeliveryDate,
                TotalAmount = po.TotalAmount,
                Status = po.Status,
                Notes = po.Notes,
                CreatedAt = po.CreatedAt
            };

            if (po.Details != null)
            {
                dto.Details = po.Details.Select(d => new PurchaseOrderDetailDto
                {
                    Id = d.Id,
                    ProductId = d.ProductId,
                    ProductName = d.Product?.ProductName ?? "Unknown",
                    ProductVariantId = d.ProductVariantId,
                    VariantName = d.ProductVariant?.VariantSku ?? "Unknown",
                    Quantity = d.Quantity,
                    UnitCost = d.UnitCost,
                    LineTotal = d.LineTotal,
                    QuantityReceived = d.QuantityReceived
                }).ToList();
            }

            return dto;
        }
    }
}
