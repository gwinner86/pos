using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs.GoodsReceipt;
using POS.Application.DTOs.SupplierInvoice;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Infrastructure.Persistence;

namespace POS.Infrastructure.Services
{
    public class GoodsReceiptService : IGoodsReceiptService
    {
        private readonly ApplicationDbContext _context;
        private readonly IInventoryService _inventoryService;
        private readonly ISupplierInvoiceService _supplierInvoiceService;

        public GoodsReceiptService(ApplicationDbContext context, IInventoryService inventoryService, ISupplierInvoiceService supplierInvoiceService)
        {
            _context = context;
            _inventoryService = inventoryService;
            _supplierInvoiceService = supplierInvoiceService;
        }

        public async Task<IEnumerable<GoodsReceiptDto>> GetGoodsReceiptsAsync(Guid tenantId, Guid companyId)
        {
            var receipts = await _context.GoodsReceipts
                .Include(gr => gr.Supplier)
                .Include(gr => gr.Location)
                .Include(gr => gr.Details)
                    .ThenInclude(d => d.Product)
                .Include(gr => gr.Details)
                    .ThenInclude(d => d.ProductVariant)
                .Where(gr => gr.TenantId == tenantId && gr.CompanyId == companyId)
                .OrderByDescending(gr => gr.CreatedAt) // NEW Sorting
                .ToListAsync();

            return receipts.Select(MapToDto).ToList();
        }

        public async Task<GoodsReceiptDto> GetGoodsReceiptByIdAsync(Guid id, Guid tenantId)
        {
            var receipt = await _context.GoodsReceipts
                .Include(gr => gr.Supplier)
                .Include(gr => gr.Location)
                .Include(gr => gr.Details)
                    .ThenInclude(d => d.Product)
                .Include(gr => gr.Details)
                    .ThenInclude(d => d.ProductVariant)
                .FirstOrDefaultAsync(gr => gr.Id == id && gr.TenantId == tenantId);

            if (receipt == null) throw new KeyNotFoundException("Goods Receipt not found.");

            return MapToDto(receipt);
        }

        public async Task<GoodsReceiptDto> CreateGoodsReceiptAsync(CreateGoodsReceiptDto dto, Guid tenantId, Guid userId, Guid companyId)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 1. Validate Location & Supplier
                    var locationExists = await _context.Locations.AnyAsync(l => l.Id == dto.LocationId && l.TenantId == tenantId);
                    if (!locationExists) throw new KeyNotFoundException("Location not found.");

                    if (dto.SupplierId.HasValue)
                    {
                        var supplierExists = await _context.Suppliers.AnyAsync(s => s.Id == dto.SupplierId.Value && s.TenantId == tenantId);
                        if (!supplierExists) throw new KeyNotFoundException("Supplier not found.");
                    }

                    // 2. Create Header
                    var receipt = new GoodsReceipt
                    {
                        TenantId = tenantId,
                        CompanyId = companyId,
                        LocationId = dto.LocationId,
                        SupplierId = dto.SupplierId,
                        PurchaseOrderId = dto.PurchaseOrderId,
                        ReceiptDate = dto.ReceiptDate,
                        CreatedBy = userId,
                        Details = new List<GoodsReceiptDetail>()
                    };

                    decimal totalReceived = 0;

                    // 3. Create Details
                    foreach (var itemDto in dto.Details)
                    {
                        var variant = await _context.ProductVariants
                            .Include(pv => pv.Product)
                            .FirstOrDefaultAsync(pv => pv.Id == itemDto.ProductVariantId && pv.Product!.TenantId == tenantId);
                        
                        if (variant == null) throw new KeyNotFoundException($"Product Variant {itemDto.ProductVariantId} not found.");

                        var lineTotal = itemDto.QuantityReceived * itemDto.UnitCost;
                        totalReceived += lineTotal;

                        receipt.Details.Add(new GoodsReceiptDetail
                        {
                            ProductId = variant.ProductId,
                            ProductVariantId = variant.Id,
                            QuantityReceived = itemDto.QuantityReceived,
                            UnitCost = itemDto.UnitCost,
                            LineTotal = lineTotal
                        });
                        // Removed AdjustInventoryAsync here. Inventory is only adjusted upon Approval.
                    }

                    receipt.TotalReceivedAmount = totalReceived;
                    receipt.Status = "Pending";
                    receipt.CreatedBy = userId;

                    _context.GoodsReceipts.Add(receipt);
                    await _context.SaveChangesAsync();

                    if (receipt.SupplierId.HasValue)
                    {
                        var invoiceDto = new CreateSupplierInvoiceDto
                        {
                            SupplierId = receipt.SupplierId.Value,
                            GoodsReceiptId = receipt.Id,
                            InvoiceNumber = $"INV-{receipt.Id.ToString().Substring(0, 8).ToUpper()}",
                            InvoiceDate = receipt.ReceiptDate,
                            DueDate = receipt.ReceiptDate.AddDays(30),
                            TotalAmount = receipt.TotalReceivedAmount
                        };
                        await _supplierInvoiceService.CreateInvoiceAsync(invoiceDto, tenantId, userId, companyId);
                    }

                    await transaction.CommitAsync();

                    return await GetGoodsReceiptByIdAsync(receipt.Id, tenantId);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<GoodsReceiptDto> UpdateGoodsReceiptAsync(Guid id, UpdateGoodsReceiptDto dto, Guid tenantId, Guid userId)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var receipt = await _context.GoodsReceipts
                        .Include(gr => gr.Details)
                        .FirstOrDefaultAsync(gr => gr.Id == id && gr.TenantId == tenantId);

                    if (receipt == null) throw new KeyNotFoundException("Goods Receipt not found.");
                    if (receipt.Status != "Pending") throw new InvalidOperationException("Only Pending Goods Receipts can be updated.");

                    if (dto.ReceiptDate.HasValue) receipt.ReceiptDate = dto.ReceiptDate.Value;

                    // Sync Details
                    var dtoDetailsDict = dto.Details
                        .Where(d => d.Id.HasValue)
                        .ToDictionary(d => d.Id!.Value);

                    var toRemove = receipt.Details
                        .Where(existing => !dtoDetailsDict.ContainsKey(existing.Id))
                        .ToList();

                    foreach (var rem in toRemove)
                    {
                         // No Inventory Reverse needed - receipt is Pending
                         _context.GoodsReceiptDetails.Remove(rem);
                         receipt.Details.Remove(rem);
                    }

                    foreach (var itemDto in dto.Details)
                    {
                        if (itemDto.Id.HasValue && dtoDetailsDict.TryGetValue(itemDto.Id.Value, out _))
                        {
                            // Update
                            var existing = receipt.Details.FirstOrDefault(d => d.Id == itemDto.Id.Value);
                            if (existing != null)
                            {
                                var oldQty = existing.QuantityReceived;
                                var qtyDiff = itemDto.QuantityReceived - existing.QuantityReceived;

                                existing.QuantityReceived = itemDto.QuantityReceived;
                                existing.UnitCost = itemDto.UnitCost;
                                existing.LineTotal = itemDto.QuantityReceived * itemDto.UnitCost;
                                
                                if (existing.ProductVariantId != itemDto.ProductVariantId)
                                {
                                     // No Inventory Revert needed - receipt is Pending
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

                             var detail = new GoodsReceiptDetail
                             {
                                 GoodsReceiptId = receipt.Id,
                                 ProductId = variant.ProductId,
                                 ProductVariantId = variant.Id,
                                 QuantityReceived = itemDto.QuantityReceived,
                                 UnitCost = itemDto.UnitCost,
                                 LineTotal = itemDto.QuantityReceived * itemDto.UnitCost
                             };
                             _context.GoodsReceiptDetails.Add(detail);
                             receipt.Details.Add(detail);
                             // No Inventory Apply needed - receipt is Pending
                        }
                    }

                    receipt.TotalReceivedAmount = receipt.Details.Sum(d => d.LineTotal);

                    _context.GoodsReceipts.Update(receipt);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return await GetGoodsReceiptByIdAsync(id, tenantId);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<GoodsReceiptDto> ApproveGoodsReceiptAsync(Guid id, Guid tenantId, Guid userId, Guid companyId)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var receipt = await _context.GoodsReceipts
                        .Include(gr => gr.Details)
                            .ThenInclude(d => d.ProductVariant)
                        .FirstOrDefaultAsync(gr => gr.Id == id && gr.TenantId == tenantId);

                    if (receipt == null) throw new KeyNotFoundException("Goods Receipt not found.");
                    if (receipt.Status == "Approved") throw new InvalidOperationException("Goods Receipt is already approved.");

                    // Set Approval States
                    receipt.Status = "Approved";
                    receipt.ApprovedBy = userId;
                    receipt.ApprovedAt = DateTime.UtcNow;

                    _context.GoodsReceipts.Update(receipt);

                    // Execute Inventory Adjustments
                    foreach (var item in receipt.Details)
                    {
                        await _inventoryService.AdjustInventoryAsync(new POS.Application.DTOs.Inventory.AdjustInventoryDto
                        {
                            ProductVariantId = item.ProductVariantId,
                            LocationId = receipt.LocationId,
                            AdjustmentQuantity = item.QuantityReceived,
                            TransactionType = "GoodsReceipt",
                            Reason = $"Goods Receipt {receipt.Id.ToString().Substring(0, 8)} Approved"
                        }, tenantId, userId, companyId);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return await GetGoodsReceiptByIdAsync(receipt.Id, tenantId);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        private static GoodsReceiptDto MapToDto(GoodsReceipt gr)
        {
            var dto = new GoodsReceiptDto
            {
                Id = gr.Id,
                CompanyId = gr.CompanyId,
                LocationId = gr.LocationId,
                LocationName = gr.Location?.LocationName ?? "Unknown",
                SupplierId = gr.SupplierId,
                SupplierName = gr.Supplier?.SupplierName ?? "Unknown",
                PurchaseOrderId = gr.PurchaseOrderId,
                ReceiptDate = gr.ReceiptDate,
                TotalReceivedAmount = gr.TotalReceivedAmount,
                Status = gr.Status,
                IsInvoiced = gr.IsInvoiced,
                CreatedBy = gr.CreatedBy,
                UpdatedBy = gr.UpdatedBy,
                ApprovedBy = gr.ApprovedBy,
                ApprovedAt = gr.ApprovedAt,
                CreatedAt = gr.CreatedAt
            };

            if (gr.Details != null)
            {
                dto.Details = gr.Details.Select(d => new GoodsReceiptDetailDto
                {
                    Id = d.Id,
                    ProductId = d.ProductId,
                    ProductName = d.Product?.ProductName ?? "Unknown",
                    ProductVariantId = d.ProductVariantId,
                    VariantName = d.ProductVariant?.VariantSku ?? "Unknown",
                    QuantityReceived = d.QuantityReceived,
                    UnitCost = d.UnitCost,
                    LineTotal = d.LineTotal
                }).ToList();
            }

            return dto;
        }
    }
}
